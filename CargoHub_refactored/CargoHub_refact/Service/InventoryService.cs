public class InventoryService : IInventoryService
{
    public Task<IEnumerable<InventoryModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<InventoryModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<InventoryModel> CreateAsync(InventoryModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, InventoryModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
