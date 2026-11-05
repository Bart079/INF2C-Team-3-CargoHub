using Project.DataAccess;

public class ShipmentServiceTests
{
    private readonly ShipmentService _service = new();

    [Fact]
    public async Task CreateAsync_ThrowsWhenShipmentIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(ShipmentModel.Reference))]
    [InlineData(nameof(ShipmentModel.Shipment_Type))]
    [InlineData(nameof(ShipmentModel.Shipment_Status))]
    [InlineData(nameof(ShipmentModel.Carrier_Name))]
    [InlineData(nameof(ShipmentModel.Shipping_Method))]
    [InlineData(nameof(ShipmentModel.Payment_Type))]
    public async Task CreateAsync_ThrowsWhenRequiredTextIsEmpty(string property)
    {
        var shipment = CreateShipment();

        switch (property)
        {
            case nameof(ShipmentModel.Reference):
                shipment.Reference = "";
                break;
            case nameof(ShipmentModel.Shipment_Type):
                shipment.Shipment_Type = "";
                break;
            case nameof(ShipmentModel.Shipment_Status):
                shipment.Shipment_Status = "";
                break;
            case nameof(ShipmentModel.Carrier_Name):
                shipment.Carrier_Name = "";
                break;
            case nameof(ShipmentModel.Shipping_Method):
                shipment.Shipping_Method = "";
                break;
            case nameof(ShipmentModel.Payment_Type):
                shipment.Payment_Type = "";
                break;
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(shipment));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, -1)]
    public async Task CreateAsync_ThrowsWhenIdIsNegative(int shipmentId, int orderId)
    {
        var shipment = CreateShipment();
        shipment.ID = shipmentId;
        shipment.Order_ID = orderId;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.CreateAsync(shipment));
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Delivered")]
    public async Task CreateAsync_ThrowsWhenShipmentStatusIsInvalid(string status)
    {
        var shipment = CreateShipment();
        shipment.Shipment_Status = status;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(shipment));
    }

    [Theory]
    [InlineData("Road")]
    [InlineData("Air")]
    public async Task CreateAsync_ThrowsWhenShippingMethodIsInvalid(string method)
    {
        var shipment = CreateShipment();
        shipment.Shipping_Method = method;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(shipment));
    }

    [Theory]
    [InlineData("Prepaid")]
    [InlineData("Credit")]
    public async Task CreateAsync_ThrowsWhenPaymentTypeIsInvalid(string paymentType)
    {
        var shipment = CreateShipment();
        shipment.Payment_Type = paymentType;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(shipment));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenIdsDoNotMatch()
    {
        var shipment = CreateShipment();
        shipment.ID = 1;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(2, shipment));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalseForNegativeId()
    {
        Assert.False(await _service.UpdateAsync(-1, CreateShipment()));
    }

    [Fact]
    public async Task UpdateOrdersAsync_ThrowsWhenOrderIdsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateOrdersAsync(1, null!));
    }

    [Fact]
    public async Task UpdateOrdersAsync_ThrowsWhenOrderIdIsNotPositive()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateOrdersAsync(1, new[] { 0 }));
    }

    [Fact]
    public async Task UpdateOrdersAsync_ThrowsWhenMoreThanOneOrderIsProvided()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateOrdersAsync(1, new[] { 1, 2 }));
    }

    [Fact]
    public async Task UpdateItemsAsync_ThrowsWhenItemsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateItemsAsync(1, null!));
    }

    [Fact]
    public async Task UpdateItemsAsync_ThrowsWhenItemIsNull()
    {
        ShipmentItemModel? item = null;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateItemsAsync(1, new[] { item! }));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task UpdateItemsAsync_ThrowsWhenItemIdOrAmountIsNotPositive(
        int itemId,
        int amount)
    {
        var item = new ShipmentItemModel(0, 1, itemId, amount);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.UpdateItemsAsync(1, new[] { item }));
    }

    [Fact]
    public async Task UpdateItemsAsync_ReturnsFalseForNonPositiveShipmentId()
    {
        Assert.False(await _service.UpdateItemsAsync(
            0,
            Array.Empty<ShipmentItemModel>()));
    }

    private static ShipmentModel CreateShipment()
    {
        return new ShipmentModel
        {
            ID = 0,
            Order_ID = 1,
            Reference = "test-shipment",
            Shipment_Date = new DateTime(2026, 1, 2, 3, 4, 5),
            Shipment_Type = "Parcel",
            Shipment_Status = "Transit",
            Carrier_Name = "Test Carrier",
            Shipping_Method = "Standard",
            Payment_Type = "Manual"
        };
    }
}