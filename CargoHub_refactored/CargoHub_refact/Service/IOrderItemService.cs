public interface IOrderItemService
{
    Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id);
}