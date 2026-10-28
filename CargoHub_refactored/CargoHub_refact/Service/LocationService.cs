using Project.DataAccess;

public class LocationService : ILocationService
{
    private void ValidateLocation(LocationModel model)
{
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (model.ID <= 0)
            throw new ArgumentException("ID must be greater than 0.");

        if (model.Warehouse_ID <= 0)
            throw new ArgumentException("Warehouse ID must be greater than 0.");

        if (string.IsNullOrWhiteSpace(model.Code))
            throw new ArgumentException("Code cannot be empty.", nameof(model.Code));

        if (string.IsNullOrWhiteSpace(model.Name)) 
            throw new ArgumentException("Name cannot be empty.", nameof(model.Name));

}
    public async Task<IEnumerable<LocationModel>> GetAllAsync()
    {
        return await LocationAcces.GetAllAsync();
    }

    public async Task<LocationModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
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
        if (id <= 0)
        return false;

        ValidateLocation(model);

        return await LocationAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        return await LocationAcces.DeleteAsync(id);
    }
}
