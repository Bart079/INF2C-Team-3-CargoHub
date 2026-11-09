using System.Net;
using System.Net.Http.Json;

public class ItemGroupApiTests : IDisposable
{
    private const string Url = "/api/v1/item_groups";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<ItemGroupModel>>());
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var group = await _context.Database.CreateItemGroup();

        var response = await _context.Client.GetAsync($"{Url}/{group.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ItemGroupModel>();
        Assert.NotNull(result);
        Assert.Equal(group.ID, result.ID);
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
        var response = await _context.Client.PostAsJsonAsync(Url, ValidItemGroup());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ItemGroupModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var group = await _context.Database.CreateItemGroup();
        group.Description = "Updated description";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{group.ID}", group);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync($"{Url}/{group.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<ItemGroupModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", ValidItemGroup());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var group = await _context.Database.CreateItemGroup();

        var response = await _context.Client.DeleteAsync($"{Url}/{group.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_ExistingGroup_Returns200()
    {
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{item.Item_Group_ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ItemModel>>();
        Assert.NotNull(items);
        Assert.Contains(items, result => result.ID == item.ID);
    }

    [Fact]
    public async Task GetItems_UnknownGroup_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static ItemGroupModel ValidItemGroup() =>
        new()
        {
            Name = $"Test Group {Guid.NewGuid():N}",
            Description = "Test item group"
        };

    public void Dispose() => _context.Dispose();
}
