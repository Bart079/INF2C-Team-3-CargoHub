using Project.DataAccess;

public class ItemTypeService : IItemTypeService
{
    private void ValidateItemType(ItemTypeModel model)
    {
        if (model == null)
        throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Name))
        throw new ArgumentException("Name cannot be empty.",nameof(model.Name));

        if (string.IsNullOrWhiteSpace(model.Description)) //niet echt nodig omdat het zoiezo niet null mag zijn maar consistent
        throw new ArgumentException("Description cannot be empty.", nameof(model.Description));
    }
    public async Task<IEnumerable<ItemTypeModel>> GetAllAsync()
    {
        return await Item_typeAcces.GetAllAsync();
    }

    public async Task<ItemTypeModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await Item_typeAcces.GetByIdAsync(id);
    }

    public async Task<ItemTypeModel> CreateAsync(ItemTypeModel model)
    {
        ValidateItemType(model);

        return await Item_typeAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, ItemTypeModel model)
    {
        if (id <= 0)
        return false;

        ValidateItemType(model);

        return await Item_typeAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false; 

        return await Item_typeAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<ItemModel>(); //geeft een lege lijst

        return await Item_typeAcces.GetItemsAsync(id);
    }
}
