public interface IItemGroupService
{
    Task<IEnumerable<ItemGroupModel>> GetAllAsync();
    Task<ItemGroupModel?> GetByIdAsync(int id);
    Task<ItemGroupModel> CreateAsync(ItemGroupModel model);
    Task<bool> UpdateAsync(int id, ItemGroupModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ItemModel>> GetItemsAsync(int id);
}
