using Project.DataAccess;

public class ItemGroupService : IItemGroupService
{ //description niet nodig mag null zijn
    private void ValidateItemGroup(ItemGroupModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);

    }
    public async Task<IEnumerable<ItemGroupModel>> GetAllAsync()
    {
        return await Item_groupAcces.GetAll();
    }

    public async Task<ItemGroupModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await Item_groupAcces.GetById(id);
    }

    public async Task<ItemGroupModel> CreateAsync(ItemGroupModel model)
    {
        ValidateItemGroup(model);
        return await Item_groupAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, ItemGroupModel model)
    {
        if (id <= 0)
        return false;

        ValidateItemGroup(model);
        return await Item_groupAcces.Update(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;

        return await Item_groupAcces.Delete(id);
    }


    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var itemGroup = await Item_groupAcces.GetById(id);

        if (itemGroup == null)
            throw new KeyNotFoundException("Item group not found.");

        return await Item_groupAcces.GetItems(id);
    }

}
