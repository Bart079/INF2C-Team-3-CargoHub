using System.Net;
using System.Net.Http.Json;

public class LocationsApiTests : IDisposable
{
    private const string Url = "/api/v1/locations";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var location = await _context.Database.CreateLocation();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var locations =
            await response.Content.ReadFromJsonAsync<List<LocationModel>>();
        Assert.NotNull(locations);
        Assert.Contains(locations, result => result.ID == location.ID);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var location = await _context.Database.CreateLocation();

        var response = await _context.Client.GetAsync(
            $"{Url}/{location.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LocationModel>();
        Assert.NotNull(result);
        Assert.Equal(location.ID, result.ID);
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
        var warehouse = await _context.Database.CreateWarehouse();
        var model = ValidModel(warehouse.ID);

        var response = await _context.Client.PostAsJsonAsync(Url, model);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<LocationModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var location = await _context.Database.CreateLocation();
        location.Name = "Updated location";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{location.ID}", location);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync(
            $"{Url}/{location.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<LocationModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated location", updated.Name);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var warehouse = await _context.Database.CreateWarehouse();

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", ValidModel(warehouse.ID));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var location = await _context.Database.CreateLocation();

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{location.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static LocationModel ValidModel(int warehouseId) =>
        new()
        {
            Warehouse_ID = warehouseId,
            Code = $"LOC-{Guid.NewGuid():N}",
            Name = "Test location"
        };

    public void Dispose() => _context.Dispose();
}
