using Project.DataAccess;

public class ItemTypeServiceTests
{
    private readonly ItemTypeService _service = new();


    [Fact]
    public async Task CreateAsync_NullItemType_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(ItemTypeModel.Name), "")]
    [InlineData(nameof(ItemTypeModel.Name), " ")]
    [InlineData(nameof(ItemTypeModel.Description), "")]
    [InlineData(nameof(ItemTypeModel.Description), " ")]
    public async Task CreateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property,
        string value)
    {
        var itemType = ValidItemType();

        switch (property)
        {
            case nameof(ItemTypeModel.Name):
                itemType.Name = value;
                break;
            case nameof(ItemTypeModel.Description):
                itemType.Description = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(itemType));
    }

    [Fact]
    public async Task UpdateAsync_NullItemType_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateAsync(1, null!));
    }

    [Theory]
    [InlineData(nameof(ItemTypeModel.Name))]
    [InlineData(nameof(ItemTypeModel.Description))]
    public async Task UpdateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property)
    {
        var itemType = ValidItemType();

        if (property == nameof(ItemTypeModel.Name))
            itemType.Name = "";
        else
            itemType.Description = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(1, itemType));
    }

    [Fact]
    public async Task GetByIdAsync_NegativeId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_NegativeId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, ValidItemType());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_NegativeId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetItemsAsync_NegativeId_ReturnsEmptyCollection()
    {
        var result = await _service.GetItemsAsync(-1);

        Assert.Empty(result);
    }

    private static ItemTypeModel ValidItemType()
    {
        return new ItemTypeModel
        {
            ID = 1,
            Name = "Standard",
            Description = "A standard item type"
        };
    }
}
