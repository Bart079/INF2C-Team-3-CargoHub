public interface ISupplierService
{
    Task<IEnumerable<SupplierModel>> GetAllAsync();
    Task<SupplierModel?> GetByIdAsync(int id);
    Task<SupplierModel> CreateAsync(SupplierModel model);
    Task<bool> UpdateAsync(int id, SupplierModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ItemModel>> GetItemsAsync(int id);
}
