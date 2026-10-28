using Project.DataAccess;
using System.Text.RegularExpressions;
public class SupplierService : ISupplierService
{
    private void ValidateSupplier(SupplierModel model)
    {
        if (model == null)
        throw new ArgumentNullException(nameof(model));
        
        if (string.IsNullOrWhiteSpace(model.Name))
        throw new ArgumentException("Name cannot be empty.",nameof(model.Name));

        if (string.IsNullOrWhiteSpace(model.Address))
        throw new ArgumentException("Address cannot be empty.",nameof(model.Address));

        if (string.IsNullOrWhiteSpace(model.City))
            throw new ArgumentException("City cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Zip_Code))
            throw new ArgumentException("Zip code cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Country))
            throw new ArgumentException("Country cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Province))
            throw new ArgumentException("Province cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Contact_Name))
            throw new ArgumentException("Contact name cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Phone_Number))
            throw new ArgumentException("Phone number cannot be empty.");

        if (!Regex.IsMatch(model.Phone_Number, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");

        var digits = Regex.Replace(model.Phone_Number, @"\D", "");

        if (digits.Length < 8)
            throw new ArgumentException("Phone number is too short.");
    }

    public async Task<IEnumerable<SupplierModel>> GetAllAsync()
    {
        return await SupplierAcces.GetAllAsync();
    }

    public async Task<SupplierModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await SupplierAcces.GetByIdAsync(id);
    }

    public async Task<SupplierModel> CreateAsync(SupplierModel model)
    {
        ValidateSupplier(model);

        return await SupplierAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, SupplierModel model)
    {
        if (id <= 0)
            return false;

        ValidateSupplier(model);

        if (id != model.ID)
            throw new ArgumentException(
                "The ID must match the model ID.", nameof(id));

        return await SupplierAcces.UpdateAsync(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        return await SupplierAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
            return Enumerable.Empty<ItemModel>();

        return await SupplierAcces.GetItemsAsync(id);
    }
}