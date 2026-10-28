using Project.DataAccess;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class WarehouseService : IWarehouseService
{
    private void ValidateWarehouse(WarehouseModel model)
{
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Code))
            throw new ArgumentException("Code cannot be empty.", nameof(model.Code));

        if (string.IsNullOrWhiteSpace(model.Name)) 
            throw new ArgumentException("Name cannot be empty.", nameof(model.Name));
            
        if (string.IsNullOrWhiteSpace(model.Address))
            throw new ArgumentException("Address cannot be empty.", nameof(model.Address));
        
        if (string.IsNullOrWhiteSpace(model.City))
            throw new ArgumentException("City cannot be empty.", nameof(model.City));

        if (string.IsNullOrWhiteSpace(model.Zip_Code))
            throw new ArgumentException("Zip code cannot be empty.", nameof(model.Zip_Code));

        if (string.IsNullOrWhiteSpace(model.Province))
            throw new ArgumentException("Province cannot be empty.", nameof(model.Province));

        if (string.IsNullOrWhiteSpace(model.Country))
            throw new ArgumentException("Country cannot be empty.", nameof(model.Country));
        
        if (string.IsNullOrWhiteSpace(model.Contact_Name))
            throw new ArgumentException("Contact name cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Contact_Email))
            throw new ArgumentException("Email cannot be empty.");

        if (!new EmailAddressAttribute().IsValid(model.Contact_Email))
            throw new ArgumentException("Email address is invalid");

        if (string.IsNullOrWhiteSpace(model.Contact_Phone))
            throw new ArgumentException("Phone number cannot be empty.");

        if (!Regex.IsMatch(model.Contact_Phone, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");

    }
    public async Task<IEnumerable<WarehouseModel>> GetAllAsync()
    {
        return await WarehouseAcces.GetAllAsync();
    }

    public async Task<WarehouseModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
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
        if (id <= 0)
        return false;

        ValidateWarehouse(model);

        if (id != model.ID)
        throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await WarehouseAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        return await WarehouseAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<LocationModel>> GetLocationsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<LocationModel>(); //geeft een lege lijst

        return await WarehouseAcces.GetLocationsAsync(id);
    }
}
