using System.Net;
using System.Net.Http.Json;

public class InventoryApiTests : IDisposable
{
    private const string Url = "/api/v1/inventories";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        await _context.Database.CreateInventory();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var inventories =
            await response.Content.ReadFromJsonAsync<List<InventoryModel>>();
        Assert.NotNull(inventories);
        Assert.Single(inventories);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var inventory = await _context.Database.CreateInventory();

        var response = await _context.Client.GetAsync(
            $"{Url}/{inventory.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InventoryModel>();
        Assert.NotNull(result);
        Assert.Equal(inventory.ID, result.ID);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201()
    {
        var item = await _context.Database.CreateItem();
        var location = await _context.Database.CreateLocation();
        var inventory = ValidInventory(checked((int)item.ID), location.ID);

        var response = await _context.Client.PostAsJsonAsync(Url, inventory);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<InventoryModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var inventory = await _context.Database.CreateInventory();
        inventory.Quantity_On_Hand = 25;

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{inventory.ID}", inventory);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync(
            $"{Url}/{inventory.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated =
            await getResponse.Content.ReadFromJsonAsync<InventoryModel>();
        Assert.NotNull(updated);
        Assert.Equal(25, updated.Quantity_On_Hand);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var item = await _context.Database.CreateItem();
        var location = await _context.Database.CreateLocation();
        var inventory = ValidInventory(checked((int)item.ID), location.ID);
        inventory.ID = 999999;

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", inventory);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var inventory = await _context.Database.CreateInventory();

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{inventory.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static InventoryModel ValidInventory(int itemId, int locationId) =>
        new()
        {
            Item_ID = itemId,
            Location_ID = locationId,
            Quantity_On_Hand = 100,
            Quantity_Expected = 50,
            Quantity_Ordered = 25,
            Quantity_Allocated = 75
        };

    public void Dispose() => _context.Dispose();
}
