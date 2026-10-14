public class LocationService : ILocationService
{
    public Task<IEnumerable<LocationModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<LocationModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<LocationModel> CreateAsync(LocationModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, LocationModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
