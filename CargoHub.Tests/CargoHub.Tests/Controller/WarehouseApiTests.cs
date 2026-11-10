using System.Net;
using System.Net.Http.Json;

public class WarehouseApiTests : IDisposable
{
    private const string Url = "/api/v1/warehouses";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var warehouse = await _context.Database.CreateWarehouse();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var warehouses =
            await response.Content.ReadFromJsonAsync<List<WarehouseModel>>();
        Assert.NotNull(warehouses);
        Assert.Contains(warehouses, result => result.ID == warehouse.ID);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var warehouse = await _context.Database.CreateWarehouse();

        var response = await _context.Client.GetAsync(
            $"{Url}/{warehouse.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WarehouseModel>();
        Assert.NotNull(result);
        Assert.Equal(warehouse.ID, result.ID);
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
        var response = await _context.Client.PostAsJsonAsync(
            Url, ValidWarehouse());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<WarehouseModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var warehouse = await _context.Database.CreateWarehouse();
        warehouse.Name = "Updated warehouse";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{warehouse.ID}", warehouse);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync(
            $"{Url}/{warehouse.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated =
            await getResponse.Content.ReadFromJsonAsync<WarehouseModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated warehouse", updated.Name);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var warehouse = ValidWarehouse();
        warehouse.ID = 999999;

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", warehouse);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_InvalidEmail_Returns400()
    {
        var warehouse = await _context.Database.CreateWarehouse();
        var originalEmail = warehouse.Contact_Email;
        warehouse.Contact_Email = "invalid-email";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{warehouse.ID}", warehouse);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Controleer dat de mislukte update geen gegevens heeft gewijzigd.
        var getResponse = await _context.Client.GetAsync($"{Url}/{warehouse.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var persisted = await getResponse.Content.ReadFromJsonAsync<WarehouseModel>();
        Assert.NotNull(persisted);
        Assert.Equal(originalEmail, persisted.Contact_Email);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var warehouse = await _context.Database.CreateWarehouse();

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{warehouse.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLocations_Existing_Returns200()
    {
        var warehouse = await _context.Database.CreateWarehouse();
        await _context.Database.CreateLocation(warehouse);

        var response = await _context.Client.GetAsync(
            $"{Url}/{warehouse.ID}/locations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var locations =
            await response.Content.ReadFromJsonAsync<List<LocationModel>>();
        Assert.NotNull(locations);
        Assert.Single(locations);
    }

    [Fact]
    public async Task GetLocations_ExistingWithoutLocations_Returns200Empty()
    {
        var warehouse = await _context.Database.CreateWarehouse();

        var response = await _context.Client.GetAsync(
            $"{Url}/{warehouse.ID}/locations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var locations = await response.Content.ReadFromJsonAsync<List<LocationModel>>();
        Assert.NotNull(locations);
        Assert.Empty(locations);
    }

    [Fact]
    public async Task GetLocations_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/locations");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidEmail_Returns400()
    {
        var warehouse = ValidWarehouse();
        warehouse.Contact_Email = "invalid-email";

        var response = await _context.Client.PostAsJsonAsync(Url, warehouse);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithLocations_Returns409()
    {
        var warehouse = await _context.Database.CreateWarehouse();
        await _context.Database.CreateLocation(warehouse);

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{warehouse.ID}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static WarehouseModel ValidWarehouse() =>
        new()
        {
            Code = $"WH-{Guid.NewGuid():N}",
            Name = "Test Warehouse",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Contact_Phone = "0612345678",
            Contact_Email = $"warehouse-{Guid.NewGuid():N}@example.com"
        };

    public void Dispose() => _context.Dispose();
}
