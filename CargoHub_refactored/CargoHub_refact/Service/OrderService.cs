using Project.DataAccess;

public class OrderService : IOrderService
{
    private void ValidateOrder(OrderModel model)
{ //worden ids door database gegeven? 
    if (model == null)
        throw new ArgumentNullException(nameof(model));

    if (model.Client_ID <= 0)
    throw new ArgumentException("Client ID must be greater than 0.");

    if (string.IsNullOrWhiteSpace(model.Reference))
    throw new ArgumentException("Reference cannot be empty.",nameof(model.Reference));

    if (string.IsNullOrWhiteSpace(model.Customer_PO_Number))
    throw new ArgumentException("Customer PO number cannot be empty.",nameof(model.Customer_PO_Number));

    if (string.IsNullOrWhiteSpace(model.Order_Status))
    throw new ArgumentException("Order status cannot be empty.",nameof(model.Order_Status));

    if (string.IsNullOrWhiteSpace(model.Shipping_Notes))
    throw new ArgumentException("Shipping notes cannot be empty.",nameof(model.Shipping_Notes));

    if (model.Warehouse_ID <= 0)
    throw new ArgumentException("Warehouse ID must be greater than 0.");

    if (model.Ship_To_Client_ID <= 0)
        throw new ArgumentException("ship to client id must be greater than 0.");

    if (model.Bill_To_Client_ID <= 0)
        throw new ArgumentException("Bill to client id must be greater than 0.");
}
    public async Task<IEnumerable<OrderModel>> GetAllAsync()
    {
        return await OrderAcces.GetAllAsync();
    }

    public async Task<OrderModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
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
        if (id <= 0)
        return false;

        ValidateOrder(model);

        return await OrderAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;

        return await OrderAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<OrderItemModel>(); //geeft een lege lijst
        
        return await OrderAcces.GetItemsAsync(id);
    }

    public async Task<bool> UpdateItemsAsync(int id, IEnumerable<OrderItemModel> items)
    {
        if (id <= 0)
            return false;

        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return await OrderAcces.UpdateItemsAsync(id, items);
    }
}
