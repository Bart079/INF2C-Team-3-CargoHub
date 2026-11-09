using Project.DataAccess;

public class ShipmentServiceTests : IDisposable
{
    private readonly ShipmentService _service = new();
    private readonly AccessTestDatabase _database = new();

    public ShipmentServiceTests() => _database.CreateSchema();

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

    [Fact]
    public async Task CreateAsync_ValidShipment_PersistsShipment()
    {
        var shipment = CreateShipment();
        var order = await _database.CreateOrder();
        shipment.Order_ID = order.ID;

        var created = await _service.CreateAsync(shipment);
        var saved = await _service.GetByIdAsync(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(saved);
        Assert.Equal(created.Reference, saved.Reference);
    }

    [Theory]
    [InlineData("Scheduled", "Express", "Automated")]
    public async Task CreateAsync_AcceptsOtherSupportedOptions(
        string status,
        string shippingMethod,
        string paymentType)
    {
        var shipment = CreateShipment();
        var order = await _database.CreateOrder();
        shipment.Order_ID = order.ID;
        shipment.Shipment_Status = status;
        shipment.Shipping_Method = shippingMethod;
        shipment.Payment_Type = paymentType;

        var created = await _service.CreateAsync(shipment);

        Assert.Equal(status, created.Shipment_Status);
        Assert.Equal(shippingMethod, created.Shipping_Method);
        Assert.Equal(paymentType, created.Payment_Type);
    }

    [Fact]
    public async Task UpdateAsync_ExistingShipment_UpdatesShipment()
    {
        var shipment = await CreateStoredShipment();
        shipment.Reference = "updated-shipment";

        var updated = await _service.UpdateAsync(shipment.ID, shipment);
        var saved = await _service.GetByIdAsync(shipment.ID);

        Assert.True(updated);
        Assert.NotNull(saved);
        Assert.Equal("updated-shipment", saved.Reference);
    }

    [Fact]
    public async Task UpdateAsync_UnknownShipment_ReturnsFalse()
    {
        var shipment = CreateShipment();
        shipment.ID = 9999;

        var updated = await _service.UpdateAsync(9999, shipment);

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_ExistingShipment_DeletesShipment()
    {
        var shipment = await CreateStoredShipment();

        var deleted = await _service.DeleteAsync(shipment.ID);
        var saved = await _service.GetByIdAsync(shipment.ID);

        Assert.True(deleted);
        Assert.Null(saved);
    }

    [Fact]
    public async Task GetOrdersAsync_ExistingShipment_ReturnsOrder()
    {
        var shipment = await CreateStoredShipment();

        var orders = await _service.GetOrdersAsync(shipment.ID);

        Assert.Equal(new[] { shipment.Order_ID }, orders);
    }

    [Fact]
    public async Task GetItemsAsync_ExistingShipment_ReturnsItems()
    {
        var item = await _database.CreateItem();
        var shipment = await CreateStoredShipment();
        await Shipment_itemAcces.ReplaceItemsForShipment(
            shipment.ID,
            [new ShipmentItemModel(0, shipment.ID, (int)item.ID, 3)]);

        var items = (await _service.GetItemsAsync(shipment.ID)).ToList();

        var savedItem = Assert.Single(items);
        Assert.Equal((int)item.ID, savedItem.Item_ID);
        Assert.Equal(3, savedItem.Amount);
    }

    [Fact]
    public async Task UpdateOrdersAsync_ExistingOrder_UpdatesOrderRelationship()
    {
        var shipment = await CreateStoredShipment();
        var newOrder = await _database.CreateOrder();

        var updated = await _service.UpdateOrdersAsync(
            shipment.ID, new[] { newOrder.ID });
        var orders = await _service.GetOrdersAsync(shipment.ID);

        Assert.True(updated);
        Assert.Equal(new[] { newOrder.ID }, orders);
    }

    [Fact]
    public async Task UpdateOrdersAsync_UnknownOrder_ThrowsArgumentException()
    {
        var shipment = await CreateStoredShipment();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateOrdersAsync(shipment.ID, new[] { 9999 }));
    }

    [Fact]
    public async Task UpdateOrdersAsync_NoOrders_ReturnsFalse()
    {
        var shipment = await CreateStoredShipment();

        var updated = await _service.UpdateOrdersAsync(
            shipment.ID, Array.Empty<int>());

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateItemsAsync_UnknownItem_ThrowsArgumentException()
    {
        var shipment = await CreateStoredShipment();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateItemsAsync(
                shipment.ID,
                [new ShipmentItemModel(0, shipment.ID, 9999, 1)]));
    }

    [Fact]
    public async Task UpdateItemsAsync_UnknownShipment_ReturnsFalse()
    {
        var item = await _database.CreateItem();

        var updated = await _service.UpdateItemsAsync(
            9999, [new ShipmentItemModel(0, 9999, (int)item.ID, 1)]);

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateItemsAsync_ReplacesItemsAndAdjustsOrderedQuantity()
    {
        var item = await _database.CreateItem();
        var shipment = await CreateStoredShipment();
        var inventory = await InventoryAcces.Create(new InventoryModel
        {
            Item_ID = (int)item.ID,
            Location_ID = (await _database.CreateLocation()).ID,
            Quantity_On_Hand = 10,
            Quantity_Expected = 0,
            Quantity_Ordered = 4,
            Quantity_Allocated = 0
        });
        await Shipment_itemAcces.ReplaceItemsForShipment(
            shipment.ID,
            [new ShipmentItemModel(0, shipment.ID, (int)item.ID, 2)]);

        var updated = await _service.UpdateItemsAsync(
            shipment.ID,
            [
                new ShipmentItemModel(0, shipment.ID, (int)item.ID, 7),
                new ShipmentItemModel(0, shipment.ID, (int)item.ID, 5)
            ]);
        var savedInventory = await InventoryAcces.GetById(inventory.ID);
        var savedItems = (await _service.GetItemsAsync(shipment.ID)).ToList();

        Assert.True(updated);
        Assert.Equal(7, savedInventory!.Quantity_Ordered);
        Assert.Equal(5, Assert.Single(savedItems).Amount);
    }

    [Fact]
    public async Task UpdateItemsAsync_RemovingItemsClampsOrderedQuantityAtZero()
    {
        var item = await _database.CreateItem();
        var shipment = await CreateStoredShipment();
        var inventory = await InventoryAcces.Create(new InventoryModel
        {
            Item_ID = (int)item.ID,
            Location_ID = (await _database.CreateLocation()).ID,
            Quantity_On_Hand = 10,
            Quantity_Expected = 0,
            Quantity_Ordered = 1,
            Quantity_Allocated = 0
        });
        await Shipment_itemAcces.ReplaceItemsForShipment(
            shipment.ID,
            [new ShipmentItemModel(0, shipment.ID, (int)item.ID, 3)]);

        var updated = await _service.UpdateItemsAsync(
            shipment.ID, Array.Empty<ShipmentItemModel>());
        var savedInventory = await InventoryAcces.GetById(inventory.ID);
        var savedItems = await _service.GetItemsAsync(shipment.ID);

        Assert.True(updated);
        Assert.Equal(0, savedInventory!.Quantity_Ordered);
        Assert.Empty(savedItems);
    }

    [Fact]
    public async Task UpdateItemsAsync_ItemWithoutInventoryStillReplacesShipmentItems()
    {
        var item = await _database.CreateItem();
        var shipment = await CreateStoredShipment();

        var updated = await _service.UpdateItemsAsync(
            shipment.ID,
            [new ShipmentItemModel(0, shipment.ID, (int)item.ID, 2)]);
        var savedItems = (await _service.GetItemsAsync(shipment.ID)).ToList();

        Assert.True(updated);
        Assert.Equal(2, Assert.Single(savedItems).Amount);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        Assert.Null(await _service.GetByIdAsync(-1));
    }

    [Fact]
    public async Task GetOrders_InvalidId_ReturnsEmpty()
    {
        Assert.Empty(await _service.GetOrdersAsync(-1));
    }

    [Fact]
    public async Task GetItems_InvalidId_ReturnsEmpty()
    {
        Assert.Empty(await _service.GetItemsAsync(-1));
    }

    private async Task<ShipmentModel> CreateStoredShipment()
    {
        var order = await _database.CreateOrder();
        var shipment = CreateShipment();
        shipment.Order_ID = order.ID;
        return await ShipmentAcces.Create(shipment);
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

    public void Dispose() => _database.Dispose();
}