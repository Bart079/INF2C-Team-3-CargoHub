using Project.DataAccess;
//misschien 2 parameters nodig om te zoeken location id en item id
public class InventoryService : IInventoryService
{
    private void ValidateInventory(InventoryModel model)
{
    ArgumentNullException.ThrowIfNull(model);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Quantity_On_Hand);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Quantity_Expected);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Quantity_Ordered);
    ArgumentOutOfRangeException.ThrowIfNegative(model.Quantity_Allocated);

    if (ItemAcces.ValidateById(model.Item_ID))
        throw new ArgumentException("Item ID is not valid / existing.", nameof(model.Item_ID));

    if (LocationAcces.ValidateById(model.Location_ID))
        throw new ArgumentException("Location ID is not valid / existing.", nameof(model.Location_ID));
}
    public async Task<IEnumerable<InventoryModel>> GetAllAsync()
    {
        return await InventoryAcces.GetAllAsync();
    }

    public async Task<InventoryModel?> GetByIdAsync(int id)
    {
        if (id < 0)
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
        if (id < 0)
        return false;
        
        ValidateInventory(model);

        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));

        return await InventoryAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
        return false;

        return await InventoryAcces.DeleteAsync(id);
    }
}
