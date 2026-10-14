public class WarehouseService : IWarehouseService
{
    public Task<IEnumerable<WarehouseModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<WarehouseModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<WarehouseModel> CreateAsync(WarehouseModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, WarehouseModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LocationModel>> GetLocationsAsync(int id)
    {
        throw new NotImplementedException();
    }
}
