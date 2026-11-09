using System.Net;
using System.Net.Http.Json;

public class ItemApiTests : IDisposable
{
    private const string Url = "/api/v1/items";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(await response.Content.ReadFromJsonAsync<List<ItemModel>>());
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync($"{Url}/{item.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ItemModel>();
        Assert.NotNull(result);
        Assert.Equal(item.ID, result.ID);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var item = await CreateValidItem();

        var response = await _context.Client.PostAsJsonAsync(Url, item);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ItemModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);

        var getResponse = await _context.Client.GetAsync($"{Url}/{created.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var item = await _context.Database.CreateItem();
        item.Description = "Updated description";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{item.ID}", item);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync($"{Url}/{item.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<ItemModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var item = await CreateValidItem();

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", item);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.DeleteAsync($"{Url}/{item.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ExistingItem_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{item.ID}/inventory");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<InventoryModel>>());
    }

    [Fact]
    public async Task GetInventory_UnknownItem_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/inventory");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInventoryTotals_ExistingItem_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{item.ID}/inventory/totals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<InventoryTotalsModel>());
    }

    [Fact]
    public async Task GetInventoryTotals_UnknownItem_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/inventory/totals");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ItemModel> CreateValidItem()
    {
        var item = await _context.Database.CreateItem();
        item.ID = 0;
        item.Code = $"ITEM-{Guid.NewGuid():N}";
        return item;
    }

    public void Dispose() => _context.Dispose();
}
