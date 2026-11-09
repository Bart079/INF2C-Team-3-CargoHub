using System.Net;
using System.Net.Http.Json;

public class ItemLineApiTests : IDisposable
{
    private const string Url = "/api/v1/item_lines";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<ItemLineModel>>());
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var line = await _context.Database.CreateItemLine();

        var response = await _context.Client.GetAsync($"{Url}/{line.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ItemLineModel>();
        Assert.NotNull(result);
        Assert.Equal(line.ID, result.ID);
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
        var response = await _context.Client.PostAsJsonAsync(Url, ValidItemLine());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ItemLineModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var line = await _context.Database.CreateItemLine();
        line.Name = $"Updated Line {Guid.NewGuid():N}";
        line.Description = "Updated description";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{line.ID}", line);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync($"{Url}/{line.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<ItemLineModel>();
        Assert.NotNull(updated);
        Assert.Equal(line.Name, updated.Name);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", ValidItemLine());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var line = await _context.Database.CreateItemLine();

        var response = await _context.Client.DeleteAsync($"{Url}/{line.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_ExistingLine_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{item.Item_Line_ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ItemModel>>();
        Assert.NotNull(items);
        Assert.Contains(items, result => result.ID == item.ID);
    }

    [Fact]
    public async Task GetItems_UnknownLine_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static ItemLineModel ValidItemLine() =>
        new()
        {
            Name = $"Test Line {Guid.NewGuid():N}",
            Description = "Test item line"
        };

    public void Dispose() => _context.Dispose();
}
