public class ShipmentService : IShipmentService
{
    public Task<IEnumerable<ShipmentModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ShipmentModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ShipmentModel> CreateAsync(ShipmentModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, ShipmentModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderModel>> GetOrdersAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ShipmentItemModel>> GetItemsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateOrdersAsync(int id, IEnumerable<int> orderIds)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateItemsAsync(int id, IEnumerable<ShipmentItemModel> items)
    {
        throw new NotImplementedException();
    }
}
