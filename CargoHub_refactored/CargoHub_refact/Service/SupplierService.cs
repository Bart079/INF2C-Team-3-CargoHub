using Project.DataAccess;
using System.Text.RegularExpressions;
public class SupplierService : ISupplierService
{
    private void ValidateSupplier(SupplierModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Address);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.City);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Zip_Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Country);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Province);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Contact_Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Phone_Number);

        if (!Regex.IsMatch(model.Phone_Number, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");

        var digits = Regex.Replace(model.Phone_Number, @"\D", "");
        if (digits.Length < 8)
            throw new ArgumentException("Phone number is too short."); 

        if (!Regex.IsMatch(model.Zip_Code, @"^[1-9][0-9]{3}\s?[A-Za-z]{2}$"))
        {
            throw new ArgumentException("Invalid zip code.", nameof(model.Zip_Code));
        }
        
        if (!Regex.IsMatch(model.Code, @"^SUP-\d{4}$"))
        {
            throw new ArgumentException("Code must start with 'SUP-' followed by 4 digits.", nameof(model.Code));
        }
    }

    public async Task<IEnumerable<SupplierModel>> GetAllAsync()
    {
        return await SupplierAcces.GetAllAsync();
    }

    public async Task<SupplierModel?> GetByIdAsync(int id)
    {
        if (id < 0)
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
        if (id < 0)
            return false;

        ValidateSupplier(model);

        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await SupplierAcces.UpdateAsync(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
            return false;

        return await SupplierAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
            return Enumerable.Empty<ItemModel>();

        return await SupplierAcces.GetItemsAsync(id);
    }
}