public class SupplierService : ISupplierService
{
    public Task<IEnumerable<SupplierModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<SupplierModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<SupplierModel> CreateAsync(SupplierModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, SupplierModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        throw new NotImplementedException();
    }
}
