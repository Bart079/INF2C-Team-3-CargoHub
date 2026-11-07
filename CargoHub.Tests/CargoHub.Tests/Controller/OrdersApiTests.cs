using Microsoft.AspNetCore.Mvc;

public class OrdersApiTests
{
    private readonly OrdersController _controller = new(new FakeOrderService());

    private static OrderModel ValidModel() =>
        new()
        {
            ID = 1,
            Client_ID = 1,
            Reference = "ORDER-1",
            Customer_PO_Number = "PO-1",
            Order_Status = "pending",
            Shipping_Notes = "Notes"
        };

    [Fact]
    public async Task GetAll_Returns200()
    {
        var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());
        Assert.Single(Assert.IsAssignableFrom<IEnumerable<OrderModel>>(ok.Value));
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        Assert.IsType<OkObjectResult>(await _controller.GetById(1));
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.GetById(2));
    }

    [Fact]
    public async Task Create_Returns201WithRouteId()
    {
        var created = Assert.IsType<CreatedAtActionResult>(
            await _controller.Create(ValidModel()));
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(OrdersController.GetById), created.ActionName);
        Assert.Equal(2, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        Assert.IsType<NoContentResult>(await _controller.Update(1, ValidModel()));
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.Update(2, ValidModel()));
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        Assert.IsType<NoContentResult>(await _controller.Delete(1));
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.Delete(2));
    }

    [Fact]
    public async Task GetItems_Existing_Returns200()
    {
        var ok = Assert.IsType<OkObjectResult>(await _controller.GetItems(1));
        Assert.IsAssignableFrom<IEnumerable<OrderItemModel>>(ok.Value);
    }

    [Fact]
    public async Task GetItems_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.GetItems(2));
    }

    [Fact]
    public async Task UpdateItems_Existing_Returns204()
    {
        Assert.IsType<NoContentResult>(
            await _controller.UpdateItems(1, Array.Empty<OrderItemModel>()));
    }

    [Fact]
    public async Task UpdateItems_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(
            await _controller.UpdateItems(2, Array.Empty<OrderItemModel>()));
    }

    private sealed class FakeOrderService : IOrderService
    {
        public Task<IEnumerable<OrderModel>> GetAllAsync() =>
            Task.FromResult<IEnumerable<OrderModel>>([ValidModel()]);
        public Task<OrderModel?> GetByIdAsync(int id) =>
            Task.FromResult(id == 1 ? ValidModel() : null);
        public Task<OrderModel> CreateAsync(OrderModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }
        public Task<bool> UpdateAsync(int id, OrderModel model) =>
            Task.FromResult(id == 1);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(id == 1);
        public Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id) =>
            Task.FromResult<IEnumerable<OrderItemModel>>(
                [new OrderItemModel { ID = 1, Order_ID = id }]);
        public Task<bool> UpdateItemsAsync(
            int id, IEnumerable<OrderItemModel> items) =>
            Task.FromResult(id == 1);
    }
}
