using Project.DataAccess;

public class ItemGroupServiceTests
{
    private readonly ItemGroupService _service = new();
    private static ItemGroupModel ValidItemGroup()
    {
        return new ItemGroupModel
        {
            Name = "ItemGroup001",
            Description = "Test ItemGroup"
        };
    }

    [Fact]
    public async Task CreateAsync_NullItemGroup_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(ItemGroupModel.Name), "")]
    [InlineData(nameof(ItemGroupModel.Name), " ")]

    public async Task CreateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property,
        string value)
    {
        //arrange
        var itemGroup = ValidItemGroup();
        //act
        switch (property)
        {
            case nameof(ItemGroupModel.Name):
                itemGroup.Name = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }
        //assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(itemGroup));
    }

    [Fact]
    public async Task UpdateAsync_NegativeId_ReturnsFalse()
    {
        // arrange
        var itemGroup = ValidItemGroup();
        int id = -1;

        //act
        var result = await _service.UpdateAsync(id, itemGroup);

        //assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_NullItemGroup_ThrowsArgumentNullException()
    {
        // arrange
        int id = 1;
        ItemGroupModel? itemGroup = null;

        //act n assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(id, itemGroup!));
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ThrowsArgumentException()
    {
        // arrange
        var itemGroup = ValidItemGroup();
        itemGroup.Name = "";
        int id = 1;

       //act n assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(id, itemGroup));
    }

    [Fact]
    public async Task DeleteAsync_NegativeId_ReturnsFalse()
    {
        // arrange
        int id = -1;

        //act
        var result = await _service.DeleteAsync(id);

        //assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_NegativeId_ReturnsNull()
    {
        // arrange
        int id = -1;

        //act
        var result = await _service.GetByIdAsync(id);

        //assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemsAsync_NegativeId_ThrowsArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.GetItemsAsync(-1));
    }
}
