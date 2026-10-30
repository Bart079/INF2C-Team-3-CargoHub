using Project.DataAccess;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class WarehouseService : IWarehouseService
{
    private void ValidateWarehouse(WarehouseModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Address);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.City);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Zip_Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Province);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Country);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Contact_Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Contact_Email);

        if (!new EmailAddressAttribute().IsValid(model.Contact_Email))
            throw new ArgumentException("Email address is invalid");

        ArgumentException.ThrowIfNullOrWhiteSpace(model.Contact_Phone);

        if (!Regex.IsMatch(model.Contact_Phone, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");
    
        if (!Regex.IsMatch(model.Zip_Code, @"^[1-9][0-9]{3}\s?[A-Za-z]{2}$"))
        {
            throw new ArgumentException("Invalid zip code.", nameof(model.Zip_Code));
        }
}
    public async Task<IEnumerable<WarehouseModel>> GetAllAsync()
    {
        return await WarehouseAcces.GetAllAsync();
    }

    public async Task<WarehouseModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await WarehouseAcces.GetByIdAsync(id);
    }

    public async Task<WarehouseModel> CreateAsync(WarehouseModel model)
    {
        ValidateWarehouse(model);

        return await WarehouseAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, WarehouseModel model)
    {
        if (id < 0)
        return false;

        ValidateWarehouse(model);

        if (id != model.ID)
        throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await WarehouseAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await WarehouseAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<LocationModel>> GetLocationsAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<LocationModel>(); //geeft een lege lijst

        return await WarehouseAcces.GetLocationsAsync(id);
    }
}
