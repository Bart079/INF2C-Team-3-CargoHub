using Project.DataAccess;

public class OrderAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public OrderAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsOrders()
    {
        await _database.CreateOrder();

        var orders = await OrderAcces.GetAll();

        Assert.Single(orders);
    }

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOrder()
    {
        var created = await _database.CreateOrder();

        var order = await OrderAcces.GetById(created.ID);

        Assert.NotNull(order);
        Assert.Equal(created.ID, order.ID);
        Assert.Equal(created.Reference, order.Reference);
    }

    [Fact]
    public async Task GetById_UnknownOrder_ReturnsNull()
    {
        var order = await OrderAcces.GetById(999999);

        Assert.Null(order);
    }

    [Fact]
    public async Task Create_AddsOrderToDatabase()
    {
        var model = await CreateOrderModel();

        var created = await OrderAcces.Create(model);
        var stored = await OrderAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Reference, stored.Reference);
        Assert.Equal(model.Client_ID, stored.Client_ID);
    }

    [Fact]
    public async Task Update_ChangesOrder()
    {
        var order = await _database.CreateOrder();
        order.Reference = "UPDATED-ORDER";

        var updated = await OrderAcces.Update(order.ID, order);
        var stored = await OrderAcces.GetById(order.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("UPDATED-ORDER", stored.Reference);
    }

    [Fact]
    public async Task Update_UnknownOrder_ReturnsFalse()
    {
        var model = await CreateOrderModel();

        var updated = await OrderAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesOrder()
    {
        var order = await _database.CreateOrder();

        var deleted = await OrderAcces.Delete(order.ID);
        var stored = await OrderAcces.GetById(order.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownOrder_ReturnsFalse()
    {
        var deleted = await OrderAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetByClientId_ReturnsOnlyMatchingOrders()
    {
        var order = await _database.CreateOrder();
        await _database.CreateOrder();

        var orders = await OrderAcces.GetByClientId(order.Client_ID);

        Assert.Single(orders);
        Assert.Equal(order.ID, orders.Single().ID);
    }

    [Fact]
    public async Task ValidateId_ExistingOrder_ReturnsTrue()
    {
        var order = await _database.CreateOrder();

        Assert.True(await OrderAcces.ValidateId(order.ID));
    }

    [Fact]
    public async Task ValidateId_UnknownOrder_ReturnsFalse()
    {
        Assert.False(await OrderAcces.ValidateId(int.MaxValue));
    }

    [Fact]
    public async Task UpdateItems_ReplacesExistingItems()
    {
        var order = await _database.CreateOrder();
        var firstItem = await _database.CreateItem();
        var secondItem = await _database.CreateItem();
        var firstOrderItem = new OrderItemModel
        {
            Item_ID = (int)firstItem.ID,
            Amount = 2,
            Unit_Price = 4.50m
        };
        var secondOrderItem = new OrderItemModel
        {
            Item_ID = (int)secondItem.ID,
            Amount = 3,
            Unit_Price = 8.25m
        };
        await OrderAcces.UpdateItems(order.ID, new[] { firstOrderItem });

        var updated = await OrderAcces.UpdateItems(order.ID, new[] { secondOrderItem });
        var items = await OrderAcces.GetItems(order.ID);

        Assert.True(updated);
        var item = Assert.Single(items);
        Assert.Equal(secondItem.ID, item.Item_ID);
        Assert.Equal(3, item.Amount);
        Assert.Equal(8.25m, item.Unit_Price);
    }

    [Fact]
    public async Task UpdateItems_UnknownOrder_ReturnsFalse()
    {
        var updated = await OrderAcces.UpdateItems(
            int.MaxValue, Array.Empty<OrderItemModel>());

        Assert.False(updated);
    }

    [Fact]
    public async Task GetItems_ReturnsOrderItems()
    {
        var order = await _database.CreateOrder();
        var item = await _database.CreateItem();
        await OrderAcces.UpdateItems(order.ID, new[]
        {
            new OrderItemModel
            {
                Item_ID = (int)item.ID,
                Amount = 2,
                Unit_Price = 4.50m
            }
        });

        var items = await OrderAcces.GetItems(order.ID);

        var result = Assert.Single(items);
        Assert.Equal(order.ID, result.Order_ID);
        Assert.Equal(item.ID, result.Item_ID);
    }

    private async Task<OrderModel> CreateOrderModel()
    {
        var order = await _database.CreateOrder();
        return new OrderModel
        {
            Client_ID = order.Client_ID,
            Request_Date = order.Request_Date,
            Reference = $"ORDER-{Guid.NewGuid():N}",
            Customer_PO_Number = order.Customer_PO_Number,
            Order_Status = order.Order_Status,
            Shipping_Notes = order.Shipping_Notes,
            Warehouse_ID = order.Warehouse_ID,
            Ship_To_Client_ID = order.Ship_To_Client_ID,
            Bill_To_Client_ID = order.Bill_To_Client_ID
        };
    }

    public void Dispose() => _database.Dispose();
}
