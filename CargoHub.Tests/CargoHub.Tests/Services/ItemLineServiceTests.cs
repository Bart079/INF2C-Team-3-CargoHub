using Project.DataAccess;

public class ItemLineServiceTests
{
    private readonly ItemLineService _service = new();

    [Fact]
    public async Task CreateAsync_ValidItemLine_ReturnsCreatedItemLine()
    {
        var itemLine = ValidItemLine();

        var result = await _service.CreateAsync(itemLine);

        Assert.Same(itemLine, result);
    }

    [Fact]
    public async Task CreateAsync_NullItemLine_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(ItemLineModel.Name), "")]
    [InlineData(nameof(ItemLineModel.Name), " ")]
    [InlineData(nameof(ItemLineModel.Description), "")]
    [InlineData(nameof(ItemLineModel.Description), " ")]
    public async Task CreateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property,
        string value)
    {
        var itemLine = ValidItemLine();

        switch (property)
        {
            case nameof(ItemLineModel.Name):
                itemLine.Name = value;
                break;
            case nameof(ItemLineModel.Description):
                itemLine.Description = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(itemLine));
    }

    [Fact]
    public async Task UpdateAsync_NullItemLine_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateAsync(1, null!));
    }

    [Theory]
    [InlineData(nameof(ItemLineModel.Name))]
    [InlineData(nameof(ItemLineModel.Description))]
    public async Task UpdateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property)
    {
        var itemLine = ValidItemLine();

        if (property == nameof(ItemLineModel.Name))
            itemLine.Name = "";
        else
            itemLine.Description = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(1, itemLine));
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
        var result = await _service.UpdateAsync(-1, ValidItemLine());

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

    private static ItemLineModel ValidItemLine()
    {
        return new ItemLineModel
        {
            ID = 1,
            Name = "Standard",
            Description = "A standard item line"
        };
    }
}
