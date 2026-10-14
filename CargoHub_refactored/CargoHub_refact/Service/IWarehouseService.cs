public interface IWarehouseService
{
    Task<IEnumerable<WarehouseModel>> GetAllAsync();
    Task<WarehouseModel?> GetByIdAsync(int id);
    Task<WarehouseModel> CreateAsync(WarehouseModel model);
    Task<bool> UpdateAsync(int id, WarehouseModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<LocationModel>> GetLocationsAsync(int id);
}
