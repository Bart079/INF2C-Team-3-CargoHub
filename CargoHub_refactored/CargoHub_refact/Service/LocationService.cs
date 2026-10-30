using Project.DataAccess;

public class LocationService : ILocationService
{
    private void ValidateLocation(LocationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentOutOfRangeException.ThrowIfNegative(model.Warehouse_ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);

        if (WarehouseAcces.ValidateById(model.Warehouse_ID))
            throw new ArgumentException(
                "Warehouse ID is not valid / existing.",
                nameof(model.Warehouse_ID));
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

        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await LocationAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await LocationAcces.DeleteAsync(id);
    }
}
