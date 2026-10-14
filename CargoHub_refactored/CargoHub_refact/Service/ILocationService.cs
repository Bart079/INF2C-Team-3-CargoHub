public interface ILocationService
{
    Task<IEnumerable<LocationModel>> GetAllAsync();
    Task<LocationModel?> GetByIdAsync(int id);
    Task<LocationModel> CreateAsync(LocationModel model);
    Task<bool> UpdateAsync(int id, LocationModel model);
    Task<bool> DeleteAsync(int id);
}
