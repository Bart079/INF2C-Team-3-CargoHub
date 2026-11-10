using Project.DataAccess;

public class ItemLineService : IItemLineService
{
    private void ValidateItemLine(ItemLineModel model, int? excludeId = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Description);

        if (Item_lineAcces.ValidateByName(model.Name, excludeId))
        {
            throw new ArgumentException(
                "Item line name already exists.",
                nameof(model.Name));
        }
    }

    public async Task<IEnumerable<ItemLineModel>> GetAllAsync()
    {
        return await Item_lineAcces.GetAll();
    }
    public async Task<ItemLineModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await Item_lineAcces.GetById(id);
    }

    public async Task<ItemLineModel> CreateAsync(ItemLineModel model)
    {
        ValidateItemLine(model);
        return await Item_lineAcces.Create(model);
    }

    public async Task<bool> UpdateAsync(int id, ItemLineModel model)
    {
        if (id <= 0)
        return false;

        ValidateItemLine(model);
        return await Item_lineAcces.Update(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;

        return await Item_lineAcces.Delete(id);
    }


    public async Task<IEnumerable<ItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var itemLine = await Item_lineAcces.GetById(id);

        if (itemLine == null)
            throw new KeyNotFoundException("Item line not found.");

        return await Item_lineAcces.GetItems(id);
    }

}
