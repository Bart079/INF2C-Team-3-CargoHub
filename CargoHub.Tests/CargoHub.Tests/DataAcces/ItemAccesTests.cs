using Project.DataAccess;

public class ItemAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public ItemAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsItems()
    {
        await _database.CreateItem();

        var items = await ItemAcces.GetAll();

        Assert.Single(items);
    }

    [Fact]
    public async Task GetAll_WhenNoItems_ReturnsEmptyCollection()
    {
        var items = await ItemAcces.GetAll();

        Assert.Empty(items);
    }

    [Fact]
    public async Task GetById_ExistingItem_ReturnsItem()
    {
        var created = await _database.CreateItem();

        var item = await ItemAcces.GetById(created.ID);

        Assert.NotNull(item);
        Assert.Equal(created.ID, item.ID);
        Assert.Equal(created.Code, item.Code);
    }

    [Fact]
    public async Task GetById_UnknownItem_ReturnsNull()
    {
        var item = await ItemAcces.GetById(999999);

        Assert.Null(item);
    }

    [Fact]
    public async Task Create_AddsItemToDatabase()
    {
        var model = await CreateItemModel();

        var created = await ItemAcces.Create(model);
        var stored = await ItemAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Code, stored.Code);
        Assert.Equal(model.Description, stored.Description);
        Assert.Equal(model.Supplier_ID, stored.Supplier_ID);
    }

    [Fact]
    public async Task Create_NullItem_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => ItemAcces.Create(null!));
    }

    [Fact]
    public async Task Update_ChangesItem()
    {
        var item = await _database.CreateItem();
        item.Description = "Updated description";

        var updated = await ItemAcces.Update(item.ID, item);
        var stored = await ItemAcces.GetById(item.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("Updated description", stored.Description);
    }

    [Fact]
    public async Task Update_UnknownItem_ReturnsFalse()
    {
        var model = await CreateItemModel();

        var updated = await ItemAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesItem()
    {
        var item = await _database.CreateItem();

        var deleted = await ItemAcces.Delete(item.ID);
        var stored = await ItemAcces.GetById(item.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownItem_ReturnsFalse()
    {
        var deleted = await ItemAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ValidatedById_ExistingItem_ReturnsFalse()
    {
        var item = await _database.CreateItem();

        Assert.False(ItemAcces.ValidatedById((int)item.ID));
    }

    [Fact]
    public void ValidatedById_UnknownItem_ReturnsTrue()
    {
        Assert.True(ItemAcces.ValidatedById(999999));
    }

    [Fact]
    public async Task AllExist_WhenAllItemsExist_ReturnsTrue()
    {
        var item = await _database.CreateItem();

        Assert.True(await ItemAcces.AllExist(new[] { (int)item.ID }));
    }

    [Fact]
    public async Task AllExist_WhenAnItemIsMissing_ReturnsFalse()
    {
        var item = await _database.CreateItem();

        Assert.False(await ItemAcces.AllExist(new[] { (int)item.ID, 999999 }));
    }

    [Fact]
    public async Task AllExist_WhenIdsAreEmpty_ReturnsTrue()
    {
        Assert.True(await ItemAcces.AllExist(Array.Empty<int>()));
    }

    [Fact]
    public async Task GetSupplierItemsById_ReturnsOnlyMatchingItems()
    {
        var item = await _database.CreateItem();

        var items = (await ItemAcces.GetSupplierItemsById(
            (int)item.Supplier_ID)).ToList();

        Assert.Single(items);
        Assert.Equal(item.ID, items[0].ID);
    }

    private async Task<ItemModel> CreateItemModel()
    {
        var existingItem = await _database.CreateItem();

        return new ItemModel
        {
            Code = $"ITEM-{Guid.NewGuid():N}",
            Description = "Another test item",
            Barcode = "987654321",
            Model_Number = "MODEL-2",
            Commodity_Code = 321,
            Unit_Weight = 2.5m,
            Item_Line_ID = existingItem.Item_Line_ID,
            Item_Group_ID = existingItem.Item_Group_ID,
            Item_Type_ID = existingItem.Item_Type_ID,
            Min_Purchase_Quantity = 2,
            Case_Size = 5,
            Packaging_Type = "Crate",
            Order_Multiple = 2,
            Supplier_ID = existingItem.Supplier_ID,
            Supplier_SKU = "SUP-SKU-2"
        };
    }

    public void Dispose() => _database.Dispose();
}
