using Project.DataAccess;

public class ItemLineService : IItemLineService
{
    private void ValidateItemLine(ItemLineModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Description);
    }

    public async Task<ItemLineModel?> GetByIdAsync(int id)
    {
        if (id < 0)
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
        if (id < 0)
        return false;

        ValidateItemLine(model);
        return await Item_lineAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
        return false;

        return await Item_lineAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<ItemModel>(); //geeft een lege lijst

        return await Item_lineAcces.GetItemsAsync(id);
    }
}
