using Project.DataAccess;

public class ItemTypeAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public ItemTypeAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsItemTypes()
    {
        await _database.CreateItemType();

        var itemTypes = await Item_typeAcces.GetAll();

        Assert.Single(itemTypes);
    }

    [Fact]
    public async Task GetAll_WhenNoItemTypes_ReturnsEmptyCollection()
    {
        var itemTypes = await Item_typeAcces.GetAll();

        Assert.Empty(itemTypes);
    }

    [Fact]
    public async Task GetById_ExistingItemType_ReturnsItemType()
    {
        var created = await _database.CreateItemType();

        var itemType = await Item_typeAcces.GetById(created.ID);

        Assert.NotNull(itemType);
        Assert.Equal(created.ID, itemType.ID);
        Assert.Equal(created.Name, itemType.Name);
        Assert.Equal(created.Description, itemType.Description);
    }

    [Fact]
    public async Task GetById_UnknownItemType_ReturnsNull()
    {
        var itemType = await Item_typeAcces.GetById(999999);

        Assert.Null(itemType);
    }

    [Fact]
    public async Task Create_AddsItemTypeToDatabase()
    {
        var model = ValidItemType();

        var created = await Item_typeAcces.Create(model);
        var stored = await Item_typeAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Description, stored.Description);
    }

    [Fact]
    public async Task Create_NullItemType_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Item_typeAcces.Create(null!));
    }

    [Fact]
    public async Task Update_ChangesItemType()
    {
        var item = await _database.CreateItemType();
        item.Description = "Updated description";

        var updated = await Item_typeAcces.Update(item.ID, item);
        var stored = await Item_typeAcces.GetById(item.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("Updated description", stored.Description);
    }

    [Fact]
    public async Task Update_UnknownItemType_ReturnsFalse()
    {
        var model = ValidItemType();

        var updated = await Item_typeAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesItemType()
    {
        var item = await _database.CreateItemType();

        var deleted = await Item_typeAcces.Delete(item.ID);
        var stored = await Item_typeAcces.GetById(item.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownItemType_ReturnsFalse()
    {
        var deleted = await Item_typeAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ValidateById_ExistingItemType_ReturnsFalse()
    {
        var item = await _database.CreateItemType();

        Assert.False(Item_typeAcces.ValidateById(item.ID));
    }

    [Fact]
    public void ValidateById_UnknownItemType_ReturnsTrue()
    {
        Assert.True(Item_typeAcces.ValidateById(999999));
    }

    [Fact]
    public async Task ValidatedByName_ExistingName_ReturnsTrue()
    {
        var item = await _database.CreateItemType();

        Assert.True(Item_typeAcces.ValidatedByName(item.Name));
    }

    [Fact]
    public void ValidatedByName_UnknownName_ReturnsFalse()
    {
        Assert.False(Item_typeAcces.ValidatedByName("unknown item type"));
    }

    [Fact]
    public async Task GetItems_ReturnsItemsForItemType()
    {
        var item = await _database.CreateItem();

        var items = (await Item_typeAcces.GetItems((int)item.Item_Type_ID)).ToList();

        Assert.Single(items);
        Assert.Equal(item.ID, items[0].ID);
    }

    private static ItemTypeModel ValidItemType()
    {
        return new ItemTypeModel
        {
            Name = $"Test item type {Guid.NewGuid():N}",
            Description = "Test item type description"
        };
    }

    public void Dispose() => _database.Dispose();
}
