using Project.DataAccess;

public class ShipmentService : IShipmentService
{
    private void ValidateShipment(ShipmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentOutOfRangeException.ThrowIfNegative(model.ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Order_ID);
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
        return await ShipmentAcces.GetAllAsync();
    }

    public async Task<ShipmentModel?> GetByIdAsync(int id)
    {
        if (id < 0)
            return null;

        return await ShipmentAcces.GetByIdAsync(id);
    }

    public async Task<ShipmentModel> CreateAsync(ShipmentModel model)
    {
        ValidateShipment(model);

        return await ShipmentAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, ShipmentModel model)
    {
        if (id < 0)
            return false;

        ValidateShipment(model);

        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await ShipmentAcces.UpdateAsync(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await ShipmentAcces.DeleteAsync(id);
    }

    // Haalt de order-ID's van een shipment op
    public async Task<IEnumerable<int>> GetOrdersAsync(int id)
    {
        if (id < 0)
            return Enumerable.Empty<int>();

        return await ShipmentAcces.GetOrdersAsync(id);
    }

    // Haalt de items van een shipment op
    public async Task<IEnumerable<ShipmentItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
            return Enumerable.Empty<ShipmentItemModel>();

        return await ShipmentAcces.GetItemsAsync(id);
    }

    public async Task<bool> UpdateOrdersAsync(int id, IEnumerable<int> orderIds)
    {
        if (id < 0)
            return false;

        if (orderIds == null)
            throw new ArgumentNullException(nameof(orderIds));

        if (orderIds.Any(orderId => orderId < 0))
            throw new ArgumentException("Order IDs must be greater than 0.", nameof(orderIds));

        return await ShipmentAcces.UpdateOrdersAsync(id, orderIds);
    }

    public async Task<bool> UpdateItemsAsync(int id,IEnumerable<ShipmentItemModel> items)
    {
        if (id < 0)
            return false;

        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return await ShipmentAcces.UpdateItemsAsync(id, items);
    }
}
