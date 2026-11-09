using Project.DataAccess;
using System.Data;

public class TransferService : ITransferService
{
    
    private void ValidateTransfer(TransferModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.From_Location_ID);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.To_Location_ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Transfer_Status);

        // Controleer of de locaties bestaan.
        if (!LocationAcces.ValidatedById(model.From_Location_ID))
        {
            throw new ArgumentException(
                "From location ID does not exist.",
                nameof(model.From_Location_ID));
        }

        if (!LocationAcces.ValidatedById(model.To_Location_ID))
        {
            throw new ArgumentException(
                "To location ID does not exist.",
                nameof(model.To_Location_ID));
        }

        // Controleer of de bron- en doellocatie verschillen.
        if (model.From_Location_ID == model.To_Location_ID)
        {
            throw new ArgumentException(
                "From and to location cannot be the same.");
        }
    }
    public async Task<IEnumerable<TransferModel>> GetAllAsync()
    {
        return await TransferAcces.GetAll();
    }

    public async Task<TransferModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await TransferAcces.GetById(id);
    }

    public async Task<TransferModel> CreateAsync(TransferModel model)
    {
        ValidateTransfer(model);

        return await TransferAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, TransferModel model)
    {
        if (id <= 0)
        return false;

        var updatetransfer = await TransferAcces.GetById(id);

        if (updatetransfer == null)
        return false;
        
        if (string.Equals(
                updatetransfer.Transfer_Status,
                "processed",
                StringComparison.OrdinalIgnoreCase)) //voorkomt verwerkt transfer aanpassen
        return false;
        
        ValidateTransfer(model);

        if (string.Equals(
                model.Transfer_Status,
                "processed",
                StringComparison.OrdinalIgnoreCase)) //voorkomt via gewone update dat transfer op processed wordt gezet
        return false;

        return await TransferAcces.Update(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;
        
        return await TransferAcces.Delete(id);
    }

    public async Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<TransferItemModel>(); //geeft een lege lijst
        
        return await TransferAcces.GetItems(id);
    }

    public async Task<bool> CommitAsync(int id)
    {
        if (id <= 0)
            return false;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        if (!await ProcessTransfer(id, connection, transaction))
            return false;

        await transaction.CommitAsync();
        return true;
    }

    private static async Task<bool> ProcessTransfer(
        int id,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        if (!await TransferAcces.TryMarkProcessed(id, connection, transaction))
            return false;

        var transfer = await TransferAcces.GetById(id, connection, transaction);
        if (transfer == null
            || transfer.From_Location_ID == transfer.To_Location_ID)
            return false;

        var quantities = await GetTransferQuantities(id, connection, transaction);
        if (quantities == null)
            return false;

        foreach (var item in quantities)
        {
            if (!await MoveItem(item, transfer, connection, transaction))
                return false;
        }

        return true;
    }

    private static async Task<IReadOnlyList<TransferQuantity>?> GetTransferQuantities(
        int id,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var items = (await Transfer_itemAcces.GetItems(
            id, connection, transaction)).ToList();

        if (items.Count == 0 || items.Any(item =>
                item.Amount <= 0
                || item.Amount != decimal.Truncate(item.Amount)))
        {
            return null;
        }

        var quantitiesByItem = items
            .GroupBy(item => item.Item_ID)
            .Select(group => new
            {
                Item_ID = group.Key,
                Amount = group.Sum(item => item.Amount)
            })
            .ToList();

        if (quantitiesByItem.Any(item => item.Amount > int.MaxValue))
            return null;

        return quantitiesByItem
            .Select(item => new TransferQuantity(item.Item_ID, (int)item.Amount))
            .ToList();
    }

    private static async Task<bool> MoveItem(
        TransferQuantity item,
        TransferModel transfer,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var source = (await InventoryAcces.GetByItemAndLocation(
            item.ItemId, transfer.From_Location_ID, connection, transaction))
            .ToList();

        if (!HasSufficientQuantity(source, item.Amount))
            return false;

        if (!await WithdrawFromSource(
                source, item.Amount, connection, transaction))
        {
            return false;
        }

        return await DepositAtDestination(
            item, transfer.To_Location_ID, connection, transaction);
    }

    private static bool HasSufficientQuantity(
        IReadOnlyCollection<InventoryModel> source,
        int amount) =>
        !source.Any(inventory => inventory.Quantity_On_Hand < 0)
        && source.Sum(inventory => (long)inventory.Quantity_On_Hand) >= amount;

    private static async Task<bool> WithdrawFromSource(
        IEnumerable<InventoryModel> source,
        int amount,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var remaining = amount;
        foreach (var inventory in source)
        {
            var moved = Math.Min(remaining, inventory.Quantity_On_Hand);
            if (moved == 0)
                continue;

            if (!await InventoryAcces.UpdateQuantityOnHand(
                    inventory.ID,
                    inventory.Quantity_On_Hand - moved,
                    connection,
                    transaction))
            {
                return false;
            }

            remaining -= moved;
            if (remaining == 0)
                break;
        }

        return remaining == 0;
    }

    private static async Task<bool> DepositAtDestination(
        TransferQuantity item,
        int destinationLocationId,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var destination = (await InventoryAcces.GetByItemAndLocation(
            item.ItemId, destinationLocationId, connection, transaction))
            .FirstOrDefault();

        if (destination == null)
        {
            await InventoryAcces.CreateAtLocation(
                item.ItemId, destinationLocationId, item.Amount,
                connection, transaction);
            return true;
        }

        if (destination.Quantity_On_Hand > int.MaxValue - item.Amount)
            return false;

        return await InventoryAcces.UpdateQuantityOnHand(
            destination.ID,
            destination.Quantity_On_Hand + item.Amount,
            connection,
            transaction);
    }

    private sealed record TransferQuantity(int ItemId, int Amount);
}
