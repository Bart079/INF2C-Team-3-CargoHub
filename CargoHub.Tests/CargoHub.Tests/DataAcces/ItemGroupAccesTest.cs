using Project.DataAccess;

public class ItemGroupAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public ItemGroupAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsItemgroups()
    {
        await _database.CreateItemGroup();

        var itemTypes = await Item_groupAcces.GetAll();

        Assert.Single(itemTypes);
    }

    [Fact]
    public async Task GetAll_WhenNoItemGroups_ReturnsEmptyCollection()
    {
        var itemTypes = await Item_groupAcces.GetAll();

        Assert.Empty(itemTypes);
    }

    [Fact]
    public async Task GetById_ExistingItemGroup_ReturnsItemGroup()
    {
        var created = await _database.CreateItemGroup();

        var itemType = await Item_groupAcces.GetById(created.ID);

        Assert.NotNull(itemType);
        Assert.Equal(created.ID, itemType.ID);
        Assert.Equal(created.Name, itemType.Name);
        Assert.Equal(created.Description, itemType.Description);
    }

    [Fact]
    public async Task GetById_UnknownItemGroup_ReturnsNull()
    {
        var itemType = await Item_groupAcces.GetById(999999);

        Assert.Null(itemType);
    }

    [Fact]
    public async Task Create_AddsItemgroupToDatabase()
    {
        var model = ValidItemGroup();

        var created = await Item_groupAcces.Create(model);
        var stored = await Item_groupAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Description, stored.Description);
    }

    [Fact]
    public async Task Create_NullItemgroup_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Item_groupAcces.Create(null!));
    }

    [Fact]
    public async Task Update_ChangesItemgroup()
    {
        var itemgroup = await _database.CreateItemGroup();
        itemgroup.Description = "Updated description";

        var updated = await Item_groupAcces.Update(itemgroup.ID, itemgroup);
        var stored = await Item_groupAcces.GetById(itemgroup.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("Updated description", stored.Description);
    }

    [Fact]
    public async Task Update_UnknownItemGroup_ReturnsFalse()
    {
        var model = ValidItemGroup();

        var updated = await Item_groupAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesItemGroup()
    {
        var itemGroup = await _database.CreateItemGroup();

        var deleted = await Item_groupAcces.Delete(itemGroup.ID);
        var stored = await Item_groupAcces.GetById(itemGroup.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownItemGroup_ReturnsFalse()
    {
        var deleted = await Item_groupAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ValidateById_ExistingItemGroup_ReturnsFalse()
    {
        var itemGroup = await _database.CreateItemGroup();

        Assert.False(Item_groupAcces.ValidateById(itemGroup.ID));
    }

    [Fact]
    public void ValidateById_UnknownItemGroup_ReturnsTrue()
    {
        Assert.True(Item_groupAcces.ValidateById(999999));
    }

    [Fact]
    public async Task GetItems_ReturnsItemsForItemGroup()
    {
        var itemGroup = await _database.CreateItem();

        var items = (await Item_groupAcces.GetItems((int)itemGroup.Item_Group_ID)).ToList();

        Assert.Single(items);
        Assert.Equal(itemGroup.ID, items[0].ID);
    }

    private static ItemGroupModel ValidItemGroup()
    {
        return new ItemGroupModel
        {
            Name = $"Test item group {Guid.NewGuid():N}",
            Description = "Test item group description"
        };
    }

    public void Dispose() => _database.Dispose();
}
