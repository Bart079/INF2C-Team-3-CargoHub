using Project.DataAccess;

public class OrderItemService : IOrderItemService
{
public async Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<OrderItemModel>(); //geeft een lege lijst

        return await OrderAcces.GetItemsAsync(id);
    }
}