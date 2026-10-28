using Project.DataAccess;

public class ItemService : IItemService
{ //mogen sommige 0 zijn? of minimaal 1
    private void ValidateItem(ItemModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Code))
            throw new ArgumentException("Code cannot be empty.", nameof(model.Code));

        if (string.IsNullOrWhiteSpace(model.Description)) //niet echt nodig omdat het zoiezo niet null mag zijn maar consistent
            throw new ArgumentException("Description cannot be empty.", nameof(model.Description));
            
        if (string.IsNullOrWhiteSpace(model.Model_Number))
            throw new ArgumentException("Model number cannot be empty.", nameof(model.Model_Number));
        
        if (model.Commodity_Code <= 0)
            throw new ArgumentException("Commodity code must be greater than zero.", nameof(model.Commodity_Code));

        if (string.IsNullOrWhiteSpace(model.Barcode))
            throw new ArgumentException("Barcode cannot be empty.", nameof(model.Barcode));

        if (model.Unit_Weight < 0)
            throw new ArgumentException("Unit weight cannot be negative.", nameof(model.Unit_Weight));

        if (model.Item_Line_ID <= 0)
            throw new ArgumentException("Item line ID must be greater than zero.", nameof(model.Item_Line_ID));

        if (model.Item_Group_ID <= 0)
            throw new ArgumentException("Item group ID must be greater than zero.", nameof(model.Item_Group_ID));

        if (model.Item_Type_ID <= 0)
            throw new ArgumentException("Item type ID must be greater than zero.", nameof(model.Item_Type_ID));

        if (model.Min_Purchase_Quantity <= 0)
            throw new ArgumentException("Minimum purchase quantity must be greater than zero.", nameof(model.Min_Purchase_Quantity));

        if (model.Case_Size <= 0)
            throw new ArgumentException("Case size must be greater than zero.", nameof(model.Case_Size));

        if (string.IsNullOrWhiteSpace(model.Packaging_Type))
            throw new ArgumentException("Packaging type cannot be empty.", nameof(model.Packaging_Type));

        if (model.Order_Multiple <= 0)
            throw new ArgumentException("Order multiple must be greater than zero.", nameof(model.Order_Multiple));

        if (model.Supplier_ID <= 0)
            throw new ArgumentException("Supplier ID must be greater than zero.", nameof(model.Supplier_ID));
    }
    public async Task<IEnumerable<ItemModel>> GetAllAsync()
    {
        return await ItemAcces.GetAllAsync();
    }

    public async Task<ItemModel?> GetByIdAsync(long id)
    {
        if (id <= 0)
        return null;

        return await ItemAcces.GetByIdAsync(id);
    }

    public async Task<ItemModel> CreateAsync(ItemModel model)
    {
        ValidateItem(model);

        return await ItemAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(long id, ItemModel model)
    {
        if (id <= 0)
        return false;

        ValidateItem(model);

        return await ItemAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        if (id <= 0)
        return false;
        
        return await ItemAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<InventoryModel>?> GetInventoryAsync(long id)
    {
        if (id <= 0) //misschien bij ongeldige id null
        return Enumerable.Empty<InventoryModel>(); //geeft een lege lijst
        
        return await InventoryAcces.GetInventoryAsync(id);
    }
//object? kan beter naar een eigen model(inventorytotalsmodel?) 
    public async Task<object?> GetInventoryTotalsAsync(long id)
    {
        if (id <= 0)
        return null;

        return await InventoryAcces.GetInventoryTotalsAsync(id);
    }
}
