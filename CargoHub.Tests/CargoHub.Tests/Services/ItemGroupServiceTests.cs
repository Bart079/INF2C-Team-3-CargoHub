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
    public async Task CreateAsync_ValidItemGroup_ReturnsCreatedItem()
    {
        //arrange
        var itemGroup = ValidItemGroup();
        //
        var result = await _service.CreateAsync(itemGroup);

        Assert.Same(itemGroup, result);
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
}
