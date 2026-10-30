using Project.DataAccess;

public class ItemService : IItemService
{ //mogen sommige 0 zijn? of minimaal 1
    private void ValidateItem(ItemModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentException.ThrowIfNullOrWhiteSpace(model.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Model_Number);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(model.Commodity_Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Barcode);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Unit_Weight);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Item_Type_ID);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Min_Purchase_Quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Case_Size);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Packaging_Type);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Order_Multiple);
        ArgumentOutOfRangeException.ThrowIfNegative(model.Supplier_ID);

        if (Item_lineAcces.ValidateById(model.Item_Line_ID))
            throw new ArgumentException("Item line ID is not valid / existing.", nameof(model.Item_Line_ID));

        if (Item_groupAcces.ValidateById(model.Item_Group_ID))
            throw new ArgumentException("Item group ID is not valid / existing.", nameof(model.Item_Group_ID));

        if (Item_typeAcces.ValidateById(model.Item_Type_ID))
            throw new ArgumentException("Item type ID is not valid / existing.", nameof(model.Item_Type_ID));

        if (SupplierAcces.ValidateById(model.Supplier_ID))
            throw new ArgumentException("Item type ID is not valid / existing.", nameof(model.Item_Type_ID));
    }

    public async Task<IEnumerable<ItemModel>> GetAllAsync()
    {
        return await ItemAcces.GetAllAsync();
    }

    public async Task<ItemModel?> GetByIdAsync(long id)
    {
        return await ItemAcces.GetByIdAsync(id);
    }

    public async Task<ItemModel> CreateAsync(ItemModel model)
    {
        ValidateItem(model);

        return await ItemAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(long id, ItemModel model)
    {
        if (id < 0)
        return false;

        ValidateItem(model);    
            
        if (id != model.ID)
            throw new ArgumentException("The ID must match the model ID.", nameof(id));



        return await ItemAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        if (id < 0)
        return false;
        
        return await ItemAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<InventoryModel>?> GetInventoryAsync(long id)
    {
        if (id < 0) //misschien bij ongeldige id null
        return Enumerable.Empty<InventoryModel>(); //geeft een lege lijst
        
        return await InventoryAcces.GetInventoryAsync(id);
    }
    public async Task<InventoryTotalsModel?> GetInventoryTotalsAsync(long id)
    {
        if (id < 0)
        return null;

        return await InventoryAcces.GetInventoryTotalsAsync(id);
    }
}
