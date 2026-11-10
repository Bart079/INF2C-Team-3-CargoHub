using Project.DataAccess;

public class LocationService : ILocationService
{
    private void ValidateLocation(LocationModel model)
    {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.ID);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.Warehouse_ID);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
    }
    public async Task<IEnumerable<LocationModel>> GetAllAsync()
    {
        return await LocationAcces.GetAll();
    }

    public async Task<LocationModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await LocationAcces.GetById(id);
    }

    public async Task<LocationModel> CreateAsync(LocationModel model)
    {
        ValidateLocation(model);

        return await LocationAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, LocationModel model)
    {
        if (id < 0)
        return false;

        ValidateLocation(model);

        return await LocationAcces.Update(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await LocationAcces.Delete(id);
    }
}
