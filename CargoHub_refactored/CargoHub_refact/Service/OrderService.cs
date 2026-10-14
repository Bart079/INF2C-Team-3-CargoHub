public class OrderService : IOrderService
{
    public Task<IEnumerable<OrderModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<OrderModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<OrderModel> CreateAsync(OrderModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, OrderModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateItemsAsync(int id, IEnumerable<OrderItemModel> items)
    {
        throw new NotImplementedException();
    }
}
