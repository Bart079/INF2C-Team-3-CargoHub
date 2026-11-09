using Project.DataAccess;

public class ItemLineAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public ItemLineAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsItemLines()
    {
        await _database.CreateItemLine();

        var ItemLines = await Item_lineAcces.GetAll();

        Assert.Single(ItemLines);
    }

    [Fact]
    public async Task GetAll_WhenNoItemLines_ReturnsEmptyCollection()
    {
        var ItemLines = await Item_lineAcces.GetAll();

        Assert.Empty(ItemLines);
    }

    [Fact]
    public async Task GetById_ExistingItemLine_ReturnsItemLine()
    {
        var created = await _database.CreateItemLine();

        var itemType = await Item_lineAcces.GetById(created.ID);

        Assert.NotNull(itemType);
        Assert.Equal(created.ID, itemType.ID);
        Assert.Equal(created.Name, itemType.Name);
        Assert.Equal(created.Description, itemType.Description);
    }

    [Fact]
    public async Task GetById_UnknownItemLine_ReturnsNull()
    {
        var itemType = await Item_lineAcces.GetById(999999);

        Assert.Null(itemType);
    }

    [Fact]
    public async Task Create_AddsItemLineToDatabase()
    {
        var model = ValidItemLine();

        var created = await Item_lineAcces.Create(model);
        var stored = await Item_lineAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Description, stored.Description);
    }

    [Fact]
    public async Task Create_NullItemLine_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Item_lineAcces.Create(null!));
    }

    [Fact]
    public async Task Update_ChangesItemLine()
    {
        var item = await _database.CreateItemLine();
        item.Description = "Updated description";

        var updated = await Item_lineAcces.Update(item.ID, item);
        var stored = await Item_lineAcces.GetById(item.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("Updated description", stored.Description);
    }

    [Fact]
    public async Task Update_UnknownItemLine_ReturnsFalse()
    {
        var model = ValidItemLine();

        var updated = await Item_lineAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesItemLine()
    {
        var item = await _database.CreateItemLine();

        var deleted = await Item_lineAcces.Delete(item.ID);
        var stored = await Item_lineAcces.GetById(item.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownItemLine_ReturnsFalse()
    {
        var deleted = await Item_lineAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ValidateById_ExistingItemLine_ReturnsFalse()
    {
        var item = await _database.CreateItemLine();

        Assert.False(Item_lineAcces.ValidateById(item.ID));
    }

    [Fact]
    public void ValidateById_UnknownItemLine_ReturnsTrue()
    {
        Assert.True(Item_lineAcces.ValidateById(999999));
    }

    [Fact]
    public async Task ValidatedByName_ExistingName_ReturnsTrue()
    {
        var itemline = await _database.CreateItemLine();

        Assert.True(Item_lineAcces.ValidateByName(itemline.Name));
    }

    [Fact]
    public void ValidatedByName_UnknownName_ReturnsFalse()
    {
        Assert.False(Item_lineAcces.ValidateByName("unknown item line"));
    }

    [Fact]
    public async Task GetItemLines_ReturnsItemsForItemLine()
    {
        var item = await _database.CreateItem();

        var items = (await Item_lineAcces.GetItems((int)item.Item_Line_ID)).ToList();

        Assert.Single(items);
        Assert.Equal(item.ID, items[0].ID);
    }

    private static ItemLineModel ValidItemLine()
    {
        return new ItemLineModel
        {
            Name = $"Test item type {Guid.NewGuid():N}",
            Description = "Test item type description"
        };
    }

    public void Dispose() => _database.Dispose();
}
