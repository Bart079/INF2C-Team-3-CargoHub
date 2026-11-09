using System.Net;
using System.Net.Http.Json;

public class ItemTypeApiTests : IDisposable
{
    private const string Url = "/api/v1/item_types";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<ItemTypeModel>>());
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var type = await _context.Database.CreateItemType();

        var response = await _context.Client.GetAsync($"{Url}/{type.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ItemTypeModel>();
        Assert.NotNull(result);
        Assert.Equal(type.ID, result.ID);
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
        var response = await _context.Client.PostAsJsonAsync(Url, ValidItemType());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ItemTypeModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var type = await _context.Database.CreateItemType();
        type.Name = $"Updated Type {Guid.NewGuid():N}";
        type.Description = "Updated description";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{type.ID}", type);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync($"{Url}/{type.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<ItemTypeModel>();
        Assert.NotNull(updated);
        Assert.Equal(type.Name, updated.Name);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", ValidItemType());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var type = await _context.Database.CreateItemType();

        var response = await _context.Client.DeleteAsync($"{Url}/{type.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_ExistingType_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{item.Item_Type_ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ItemModel>>();
        Assert.NotNull(items);
        Assert.Contains(items, result => result.ID == item.ID);
    }

    [Fact]
    public async Task GetItems_UnknownType_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static ItemTypeModel ValidItemType() =>
        new()
        {
            Name = $"Test Type {Guid.NewGuid():N}",
            Description = "Test item type"
        };

    public void Dispose() => _context.Dispose();
}
