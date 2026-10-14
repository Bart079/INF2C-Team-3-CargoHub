public class ItemLineService : IItemLineService
{
    public Task<IEnumerable<ItemLineModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ItemLineModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ItemLineModel> CreateAsync(ItemLineModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, ItemLineModel model)
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
