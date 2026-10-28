using Project.DataAccess;
//misschien 2 parameters nodig om te zoeken location id en item id
public class InventoryService : IInventoryService
{
    private void ValidateInventory(InventoryModel model)
{
    if (model == null)
        throw new ArgumentNullException(nameof(model));

    if (model.Item_ID <= 0)
    throw new ArgumentException("Item ID must be greater than 0.");

    if (model.Location_ID <= 0)
    throw new ArgumentException("Location ID must be greater than 0.");

    if (model.Quantity_On_Hand <= 0)
        throw new ArgumentException("Quantity on hand must be greater than 0.");

    if (model.Quantity_Expected <= 0)
        throw new ArgumentException("Quantity expected must be greater than 0.");

    if (model.Quantity_Ordered <= 0)
        throw new ArgumentException("Quantity ordered must be greater than 0.");

    if (model.Quantity_Allocated <= 0)
        throw new ArgumentException("Quantity allocated must be greater than 0.");
}
    public async Task<IEnumerable<InventoryModel>> GetAllAsync()
    {
        return await InventoryAcces.GetAllAsync();
    }

    public async Task<InventoryModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await InventoryAcces.GetByIdAsync(id);
    }

    public async Task<InventoryModel> CreateAsync(InventoryModel model)
    {
        ValidateInventory(model);

        return await InventoryAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, InventoryModel model)
    {
        if (id <= 0)
        return false;

        ValidateInventory(model);

        return await InventoryAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        return false;

        return await InventoryAcces.DeleteAsync(id);
    }
}
