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
        return await Item_groupAcces.GetAllAsync();
    }

    public async Task<ItemGroupModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await Item_groupAcces.GetByIdAsync(id);
    }

    public async Task<ItemGroupModel> CreateAsync(ItemGroupModel model)
    {
        ValidateItemGroup(model);
        return await Item_groupAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, ItemGroupModel model)
    {
        if (id < 0)
        return false;

        ValidateItemGroup(model);
        return await Item_groupAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
        return false;

        return await Item_groupAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<ItemModel>(); //geeft een lege lijst
        
        return await Item_groupAcces.GetItemsAsync(id);
    }
}
