using System.Net;
using System.Net.Http.Json;

public class OrdersApiTests : IDisposable
{
    private const string Url = "/api/v1/orders";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var order = await _context.Database.CreateOrder();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderModel>>();
        Assert.NotNull(orders);
        Assert.Contains(orders, result => result.ID == order.ID);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var order = await _context.Database.CreateOrder();

        var response = await _context.Client.GetAsync($"{Url}/{order.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<OrderModel>();
        Assert.NotNull(result);
        Assert.Equal(order.ID, result.ID);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201WithRouteId()
    {
        var model = await CreateValidOrder();

        var response = await _context.Client.PostAsJsonAsync(Url, model);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var order = await _context.Database.CreateOrder();
        order.Shipping_Notes = "Updated notes";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{order.ID}", order);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync($"{Url}/{order.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<OrderModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated notes", updated.Shipping_Notes);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var model = await CreateValidOrder();

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", model);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var order = await _context.Database.CreateOrder();

        var response = await _context.Client.DeleteAsync($"{Url}/{order.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_Existing_Returns200()
    {
        var order = await _context.Database.CreateOrder();

        var response = await _context.Client.GetAsync(
            $"{Url}/{order.ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<OrderItemModel>>());
    }

    [Fact]
    public async Task GetItems_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItems_Existing_Returns204()
    {
        var order = await _context.Database.CreateOrder();

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{order.ID}/items", Array.Empty<OrderItemModel>());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItems_Unknown_Returns404()
    {
        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999/items", Array.Empty<OrderItemModel>());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<OrderModel> CreateValidOrder()
    {
        var order = await _context.Database.CreateOrder();
        order.ID = 0;
        order.Reference = $"ORDER-{Guid.NewGuid():N}";
        return order;
    }

    public void Dispose() => _context.Dispose();
}
