public interface IItemLineService
{
    Task<IEnumerable<ItemLineModel>> GetAllAsync();
    Task<ItemLineModel?> GetByIdAsync(int id);
    Task<ItemLineModel> CreateAsync(ItemLineModel model);
    Task<bool> UpdateAsync(int id, ItemLineModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ItemModel>> GetItemsAsync(int id);
}
