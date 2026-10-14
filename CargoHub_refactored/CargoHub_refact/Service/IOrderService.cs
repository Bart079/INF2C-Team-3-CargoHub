public interface IOrderService
{
    Task<IEnumerable<OrderModel>> GetAllAsync();
    Task<OrderModel?> GetByIdAsync(int id);
    Task<OrderModel> CreateAsync(OrderModel model);
    Task<bool> UpdateAsync(int id, OrderModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id);
    Task<bool> UpdateItemsAsync(int id, IEnumerable<OrderItemModel> items);
}
