public interface IItemTypeService
{
    Task<IEnumerable<ItemTypeModel>> GetAllAsync();
    Task<ItemTypeModel?> GetByIdAsync(int id);
    Task<ItemTypeModel> CreateAsync(ItemTypeModel model);
    Task<bool> UpdateAsync(int id, ItemTypeModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ItemModel>> GetItemsAsync(int id);
}
