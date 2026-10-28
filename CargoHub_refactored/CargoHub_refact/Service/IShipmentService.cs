public interface IShipmentService
{
    Task<IEnumerable<ShipmentModel>> GetAllAsync();
    Task<ShipmentModel?> GetByIdAsync(int id);
    Task<ShipmentModel> CreateAsync(ShipmentModel model);
    Task<bool> UpdateAsync(int id, ShipmentModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<int>> GetOrdersAsync(int id);
    Task<IEnumerable<ShipmentItemModel>> GetItemsAsync(int id);
    Task<bool> UpdateOrdersAsync(int id, IEnumerable<int> orderIds);
    Task<bool> UpdateItemsAsync(int id, IEnumerable<ShipmentItemModel> items);
}
