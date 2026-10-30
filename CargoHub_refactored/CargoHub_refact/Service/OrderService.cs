using Project.DataAccess;

public class OrderService : IOrderService
{
    private void ValidateOrder(OrderModel model)
{ 
    ArgumentNullException.ThrowIfNull(model);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Client_ID);
    ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
    ArgumentException.ThrowIfNullOrWhiteSpace(model.Customer_PO_Number);
    ArgumentException.ThrowIfNullOrWhiteSpace(model.Order_Status);
    ArgumentException.ThrowIfNullOrWhiteSpace(model.Shipping_Notes);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Warehouse_ID);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Ship_To_Client_ID);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Bill_To_Client_ID);

    if (model.Order_Status != "shipped" && model.Order_Status != "pending")
        {
            throw new ArgumentException("Invalid order status.", nameof(model.Order_Status));
        }
}
    public async Task<IEnumerable<OrderModel>> GetAllAsync()
    {
        return await OrderAcces.GetAllAsync();
    }

    public async Task<OrderModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await OrderAcces.GetByIdAsync(id);
    }
    public async Task<OrderModel> CreateAsync(OrderModel model)
    {
        ValidateOrder(model);

        return await OrderAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, OrderModel model)
    {
        if (id < 0)
        return false;

        ValidateOrder(model);

        return await OrderAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
        return false;

        return await OrderAcces.DeleteAsync(id);
    }

    //checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<OrderModel>> GetOrdersAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<OrderItemModel>(); //geeft een lege lijst
        
        return await OrderAcces.GetItemsAsync(id);
    }

    public async Task<bool> UpdateItemsAsync(int id, IEnumerable<OrderItemModel> items)
    {
        if (id < 0)
            return false;

        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return await OrderAcces.UpdateItemsAsync(id, items);
    }
}
