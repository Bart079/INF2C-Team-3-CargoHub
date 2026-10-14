public class ItemService : IItemService
{
    public Task<IEnumerable<ItemModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ItemModel?> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<ItemModel> CreateAsync(ItemModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(long id, ItemModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryModel>?> GetInventoryAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<object?> GetInventoryTotalsAsync(long id)
    {
        throw new NotImplementedException();
    }
}
