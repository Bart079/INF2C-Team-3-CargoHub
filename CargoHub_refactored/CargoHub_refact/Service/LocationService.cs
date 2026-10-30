using Project.DataAccess;

public class LocationService : ILocationService
{
    private void ValidateLocation(LocationModel model)
    {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentOutOfRangeException.ThrowIfNegative(model.ID);
            ArgumentOutOfRangeException.ThrowIfNegative(model.Warehouse_ID);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
    }
    public async Task<IEnumerable<LocationModel>> GetAllAsync()
    {
        return await LocationAcces.GetAllAsync();
    }

    public async Task<LocationModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await LocationAcces.GetByIdAsync(id );
    }

    public async Task<LocationModel> CreateAsync(LocationModel model)
    {
        ValidateLocation(model);

        return await LocationAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, LocationModel model)
    {
        if (id < 0)
        return false;

        ValidateLocation(model);

        return await LocationAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await LocationAcces.DeleteAsync(id);
    }
}
