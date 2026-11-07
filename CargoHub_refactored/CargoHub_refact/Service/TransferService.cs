using Project.DataAccess;

public class TransferService : ITransferService
{
    private void ValidateTransfer(TransferModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
        ArgumentOutOfRangeException.ThrowIfNegative(model.From_Location_ID);
        ArgumentOutOfRangeException.ThrowIfNegative(model.To_Location_ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Transfer_Status);

        if (LocationAcces.ValidatedById(model.From_Location_ID))
            throw new ArgumentException("From location ID is not valid / existing.", nameof(model.From_Location_ID));

        if (LocationAcces.ValidatedById(model.To_Location_ID))
            throw new ArgumentException("To location ID is not valid / existing.", nameof(model.To_Location_ID));
    }

    public async Task<IEnumerable<TransferModel>> GetAllAsync()
    {
        return await TransferAcces.GetAll();
    }

    public async Task<TransferModel?> GetByIdAsync(int id)
    {
        if (id < 0)
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
        if (id < 0)
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
        if (id < 0)
        return false;
        
        return await TransferAcces.Delete(id);
    }

    public async Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<TransferItemModel>(); //geeft een lege lijst
        
        return await TransferAcces.GetItems(id);
    }

    public async Task<bool> CommitAsync(int id)
    {
        if (id < 0)
            return false;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        if (!await TransferAcces.TryMarkProcessed(id, connection, transaction))
            return false;

        var transfer = await TransferAcces.GetById(id, connection, transaction);
        if (transfer == null
            || transfer.From_Location_ID == transfer.To_Location_ID)
        {
            return false;
        }

        var items = (await Transfer_itemAcces.GetItems(
            id, connection, transaction)).ToList();

        if (items.Count == 0
            || items.Any(item => item.Amount <= 0
                || item.Amount != decimal.Truncate(item.Amount)))
        {
            return false;
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
            return false;

        foreach (var item in quantitiesByItem)
        {
            var amount = (int)item.Amount;
            var source = (await InventoryAcces.GetByItemAndLocation(
                item.Item_ID, transfer.From_Location_ID, connection, transaction))
                .ToList();

            if (source.Any(inventory => inventory.Quantity_On_Hand < 0)
                || source.Sum(inventory => (long)inventory.Quantity_On_Hand) < amount)
            {
                return false;
            }

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

            var destination = (await InventoryAcces.GetByItemAndLocation(
                item.Item_ID, transfer.To_Location_ID, connection, transaction))
                .FirstOrDefault();

            if (destination == null)
            {
                await InventoryAcces.CreateAtLocation(
                    item.Item_ID, transfer.To_Location_ID, amount,
                    connection, transaction);
            }
            else
            {
                if (destination.Quantity_On_Hand > int.MaxValue - amount
                    || !await InventoryAcces.UpdateQuantityOnHand(
                        destination.ID,
                        destination.Quantity_On_Hand + amount,
                        connection,
                        transaction))
                {
                    return false;
                }
            }
        }

        await transaction.CommitAsync();
        return true;
    }
}
