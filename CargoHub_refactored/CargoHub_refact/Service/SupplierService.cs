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
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);

        if (!Regex.IsMatch(model.Phone_Number, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");

        var digits = Regex.Replace(model.Phone_Number, @"\D", "");
        if (digits.Length < 8)
            throw new ArgumentException("Phone number is too short.");
    }

    public async Task<IEnumerable<SupplierModel>> GetAllAsync()
    {
        return await SupplierAcces.GetAll();
    }

    public async Task<SupplierModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await SupplierAcces.GetById(id);
    }

    public async Task<SupplierModel> CreateAsync(SupplierModel model)
    {
        ValidateSupplier(model);

        return await SupplierAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, SupplierModel model)
    {
        if (id <= 0)
            return false;

        ValidateSupplier(model);

        if (id != model.ID)
            throw new ArgumentException(
                "The ID must match the model ID.", nameof(id));

        return await SupplierAcces.Update(id, model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        return await SupplierAcces.Delete(id);
    }

    public async Task<IEnumerable<ItemModel>> GetSupplierItemsAsync(int id)
    {
        if (id <= 0)
            return Enumerable.Empty<ItemModel>();

        return await ItemAcces.GetSupplierItemsById(id);
    }
}