using Project.DataAccess;

public class ShipmentService : IShipmentService
{
    private void ValidateShipment(ShipmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.Order_ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Shipment_Type);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Shipment_Status);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Carrier_Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Shipping_Method);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Payment_Type);

        if (model.Shipment_Status != "Transit" && model.Shipment_Status != "Scheduled")
        {
            throw new ArgumentException("Invalid shipment status.", nameof(model.Shipment_Status));
        }

        if (model.Shipping_Method != "Express" && model.Shipping_Method != "Standard")
        {
            throw new ArgumentException("Invalid shipping method.", nameof(model.Shipping_Method));
        }

        if (model.Payment_Type != "Automated" && model.Payment_Type != "Manual")
        {
            throw new ArgumentException("Invalid payment type.", nameof(model.Payment_Type));
        }
    }

    public async Task<IEnumerable<ShipmentModel>> GetAllAsync()
    {
        return await ShipmentAcces.GetAll();
    }

    public async Task<ShipmentModel?> GetByIdAsync(int id)
    {
        if (id < 0)
            return null;

        return await ShipmentAcces.GetById(id);
    }

    public async Task<ShipmentModel> CreateAsync(ShipmentModel model)
    {
        ValidateShipment(model);

        return await ShipmentAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, ShipmentModel model)
    {
        if (id < 0)
            return false;

        ValidateShipment(model);

        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await ShipmentAcces.Update(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await ShipmentAcces.Delete(id);
    }

    // Haalt de order-ID's van een shipment op
    public async Task<IEnumerable<int>> GetOrdersAsync(int id)
    {
        if (id < 0)
            return Enumerable.Empty<int>();

        return await ShipmentAcces.GetOrders(id);
    }

    // Haalt de items van een shipment op
    public async Task<IEnumerable<ShipmentItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
            return Enumerable.Empty<ShipmentItemModel>();

        return await Shipment_itemAcces.GetItemsByShipmentId(id);
    }

    public async Task<bool> UpdateOrdersAsync(int id, IEnumerable<int> orderIds)
    {
        if (id < 0)
            return false;

        ArgumentNullException.ThrowIfNull(orderIds);

        if (orderIds.Any(orderId => orderId <= 0))
            throw new ArgumentException("Order IDs must be greater than 0.", nameof(orderIds));

            var ids = orderIds.ToList();

        if (ids.Count > 1)
            throw new ArgumentException("Shipment can only have 1 order.", nameof(orderIds));

        foreach (int orderID in ids)
        {
            if (orderID <= 0)
                throw new ArgumentException("Order id must be greater than 0.", nameof(orderIds));

            if (!await OrderAcces.ValidateId(orderID))
                throw new ArgumentException($"Order ID {orderID} does not exist.", nameof(orderIds));
        }

    return await ShipmentAcces.UpdateOrders(id, ids);
    }

    public async Task<bool> UpdateItemsAsync(
        int id,
        IEnumerable<ShipmentItemModel> items)
    {
        if (id <= 0)
            return false;

        ArgumentNullException.ThrowIfNull(items);

        var itemsList = items.ToList();
        foreach (var item in itemsList)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(item.Item_ID);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(item.Amount);
        }

        var newAmounts = itemsList
            .GroupBy(item => item.Item_ID)
            .ToDictionary(group => group.Key, group => group.Last().Amount);

        if (!await ItemAcces.AllExist(newAmounts.Keys))
            throw new ArgumentException("One or more item IDs do not exist.", nameof(items));

        if (await ShipmentAcces.GetById(id) is null)
            return false;

        var currentItems = await Shipment_itemAcces.GetItemsByShipmentId(id);
        var currentAmounts = currentItems
            .GroupBy(item => item.Item_ID)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));
        var deltas = currentAmounts.Keys
            .Union(newAmounts.Keys)
            .Select(itemId => new
            {
                ItemId = itemId,
                Delta = newAmounts.GetValueOrDefault(itemId)
                    - currentAmounts.GetValueOrDefault(itemId)
            })
            .Where(item => item.Delta != 0)
            .ToDictionary(item => item.ItemId, item => item.Delta);

        var inventories = await InventoryAcces.GetByItemIds(deltas.Keys);
        var inventoriesToUpdate = inventories
            .GroupBy(inventory => inventory.Item_ID)
            .Select(group => group.OrderByDescending(
                inventory => inventory.Quantity_On_Hand).First())
            .Where(inventory => deltas.ContainsKey(inventory.Item_ID))
            .ToList();

        foreach (var inventory in inventoriesToUpdate)
        {
            inventory.Quantity_Ordered = Math.Max(
                0,
                inventory.Quantity_Ordered + deltas[inventory.Item_ID]);
        }

        if (!await InventoryAcces.UpdateOrderedQuantities(inventoriesToUpdate))
            throw new InvalidOperationException("One or more inventory rows could not be updated.");

        await Shipment_itemAcces.ReplaceItemsForShipment(
            id,
            newAmounts.Select(item => new ShipmentItemModel(
                0, id, item.Key, item.Value)));

        return true;
    }
}
