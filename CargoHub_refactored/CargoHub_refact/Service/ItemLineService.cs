using Project.DataAccess;

public class ItemLineService : IItemLineService
{
    private void ValidateItemLine(ItemLineModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Name))
            throw new ArgumentException("Name cannot be empty.", nameof(model.Name));

        if (string.IsNullOrWhiteSpace(model.Description)) //niet echt nodig omdat het zoiezo niet null mag zijn maar consistent
            throw new ArgumentException("Description cannot be empty.", nameof(model.Description));
    }
    public async Task<IEnumerable<ItemLineModel>> GetAllAsync()
    {
        return await Item_lineAcces.GetAllAsync();
    }

    public async Task<ItemLineModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await Item_lineAcces.GetByIdAsync(id);
    }

    public async Task<ItemLineModel> CreateAsync(ItemLineModel model)
    {
        ValidateItemLine(model);
        return await Item_lineAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, ItemLineModel model)
    {
        if (id <= 0)
        return false;

        ValidateItemLine(model);
        return await Item_lineAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;

        return await Item_lineAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<ItemModel>(); //geeft een lege lijst

        return await Item_lineAcces.GetItemsAsync(id);
    }
}
