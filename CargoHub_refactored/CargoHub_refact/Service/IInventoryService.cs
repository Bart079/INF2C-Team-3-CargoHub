public interface IInventoryService
{
    Task<IEnumerable<InventoryModel>> GetAllAsync();
    Task<InventoryModel?> GetByIdAsync(int id);
    Task<InventoryModel> CreateAsync(InventoryModel model);
    Task<bool> UpdateAsync(int id, InventoryModel model);
    Task<bool> DeleteAsync(int id);
}
