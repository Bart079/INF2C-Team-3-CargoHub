public interface IItemService
{
    Task<IEnumerable<ItemModel>> GetAllAsync();
    Task<ItemModel?> GetByIdAsync(long id);
    Task<ItemModel> CreateAsync(ItemModel model);
    Task<bool> UpdateAsync(long id, ItemModel model);
    Task<bool> DeleteAsync(long id);
    Task<IEnumerable<InventoryModel>?> GetInventoryAsync(long id);
    Task<object?> GetInventoryTotalsAsync(long id);
}
