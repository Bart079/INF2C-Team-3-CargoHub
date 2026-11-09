using Project.DataAccess;

public class ItemServiceTests
{
    private readonly ItemService _service = new();

    [Fact]
    public async Task CreateAsync_NullItem_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(ItemModel.Code), "")]
    [InlineData(nameof(ItemModel.Code), " ")]
    [InlineData(nameof(ItemModel.Description), "")]
    [InlineData(nameof(ItemModel.Description), " ")]
    [InlineData(nameof(ItemModel.Model_Number), "")]
    [InlineData(nameof(ItemModel.Model_Number), " ")]
    [InlineData(nameof(ItemModel.Barcode), "")]
    [InlineData(nameof(ItemModel.Barcode), " ")]
    [InlineData(nameof(ItemModel.Packaging_Type), "")]
    [InlineData(nameof(ItemModel.Packaging_Type), " ")]
    public async Task CreateAsync_RequiredTextIsEmpty_ThrowsArgumentException(
        string property,
        string value)
    {
        var item = ValidItem();

        switch (property)
        {
            case nameof(ItemModel.Code):
                item.Code = value;
                break;
            case nameof(ItemModel.Description):
                item.Description = value;
                break;
            case nameof(ItemModel.Model_Number):
                item.Model_Number = value;
                break;
            case nameof(ItemModel.Barcode):
                item.Barcode = value;
                break;
            case nameof(ItemModel.Packaging_Type):
                item.Packaging_Type = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(item));
    }

    [Theory]
    [InlineData(nameof(ItemModel.Commodity_Code))]
    [InlineData(nameof(ItemModel.Unit_Weight))]
    [InlineData(nameof(ItemModel.Item_Type_ID))]
    [InlineData(nameof(ItemModel.Min_Purchase_Quantity))]
    [InlineData(nameof(ItemModel.Case_Size))]
    [InlineData(nameof(ItemModel.Order_Multiple))]
    [InlineData(nameof(ItemModel.Supplier_ID))]
    public async Task CreateAsync_NegativeNumber_ThrowsArgumentOutOfRangeException(
        string property)
    {
        var item = ValidItem();

        switch (property)
        {
            case nameof(ItemModel.Commodity_Code):
                item.Commodity_Code = -1;
                break;
            case nameof(ItemModel.Unit_Weight):
                item.Unit_Weight = -1;
                break;
            case nameof(ItemModel.Item_Type_ID):
                item.Item_Type_ID = -1;
                break;
            case nameof(ItemModel.Min_Purchase_Quantity):
                item.Min_Purchase_Quantity = -1;
                break;
            case nameof(ItemModel.Case_Size):
                item.Case_Size = -1;
                break;
            case nameof(ItemModel.Order_Multiple):
                item.Order_Multiple = -1;
                break;
            case nameof(ItemModel.Supplier_ID):
                item.Supplier_ID = -1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property));
        }

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.CreateAsync(item));
    }

    [Fact]
    public async Task CreateAsync_ZeroCommodityCode_ThrowsArgumentOutOfRangeException()
    {
        var item = ValidItem();
        item.Commodity_Code = 0;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.CreateAsync(item));
    }

    [Fact]
    public async Task UpdateAsync_NullItem_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateAsync(1, null!));
    }

    [Fact]
    public async Task UpdateAsync_EmptyCode_ThrowsArgumentException()
    {
        var item = ValidItem();
        item.Code = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(1, item));
    }

    [Fact]
    public async Task UpdateAsync_NegativeId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, ValidItem());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_NegativeId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetInventoryAsync_NegativeId_ReturnsEmptyCollection()
    {
        var result = await _service.GetInventoryAsync(-1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInventoryTotalsAsync_NegativeId_ReturnsNull()
    {
        var result = await _service.GetInventoryTotalsAsync(-1);

        Assert.Null(result);
    }

    private static ItemModel ValidItem()
    {
        return new ItemModel
        {
            Code = "ITEM001",
            Description = "Test item",
            Barcode = "1234567890123",
            Model_Number = "MODEL001",
            Commodity_Code = 1234,
            Unit_Weight = 1.5m,
            Item_Line_ID = 1,
            Item_Group_ID = 1,
            Item_Type_ID = 1,
            Min_Purchase_Quantity = 1,
            Case_Size = 10,
            Packaging_Type = "Box",
            Order_Multiple = 1,
            Supplier_ID = 1,
            Supplier_SKU = "SUP-ITEM001"
        };
    }
}
