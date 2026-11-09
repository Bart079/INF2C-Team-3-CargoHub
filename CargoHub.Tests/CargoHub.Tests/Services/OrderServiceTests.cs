public class OrderServiceTests
{
    private readonly OrderService _service = new();

    private static OrderModel ValidOrder()
    {
        return new OrderModel
        {
            Client_ID = 1,
            Reference = "ORDER-1",
            Customer_PO_Number = "PO-1",
            Order_Status = "pending",
            Shipping_Notes = "Test notes",
            Warehouse_ID = 1,
            Ship_To_Client_ID = 1,
            Bill_To_Client_ID = 1
        };
    }

    [Fact]
    public async Task Create_NullModel_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(OrderModel.Reference))]
    [InlineData(nameof(OrderModel.Customer_PO_Number))]
    [InlineData(nameof(OrderModel.Order_Status))]
    [InlineData(nameof(OrderModel.Shipping_Notes))]
    public async Task Create_EmptyRequiredText_ThrowsArgumentException(string property)
    {
        var model = ValidOrder();

        switch (property)
        {
            case nameof(OrderModel.Reference):
                model.Reference = "";
                break;
            case nameof(OrderModel.Customer_PO_Number):
                model.Customer_PO_Number = "";
                break;
            case nameof(OrderModel.Order_Status):
                model.Order_Status = "";
                break;
            case nameof(OrderModel.Shipping_Notes):
                model.Shipping_Notes = "";
                break;
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_InvalidStatus_ThrowsArgumentException()
    {
        var model = ValidOrder();
        model.Order_Status = "processing";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetItems_InvalidId_ReturnsEmpty()
    {
        var result = await _service.GetItemsAsync(-1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateItems_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateItemsAsync(-1, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateItems_NullItems_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateItemsAsync(1, null!));
    }
}
