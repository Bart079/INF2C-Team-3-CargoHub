using System.Net;
using System.Net.Http.Json;

public class SupplierApiTests : IDisposable
{
    private const string Url = "/api/v1/suppliers";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var supplier = await _context.Database.CreateSupplier();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suppliers =
            await response.Content.ReadFromJsonAsync<List<SupplierModel>>();
        Assert.NotNull(suppliers);
        Assert.Contains(suppliers, result => result.ID == supplier.ID);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var supplier = await _context.Database.CreateSupplier();

        var response = await _context.Client.GetAsync(
            $"{Url}/{supplier.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SupplierModel>();
        Assert.NotNull(result);
        Assert.Equal(supplier.ID, result.ID);
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
        var response = await _context.Client.PostAsJsonAsync(
            Url, ValidSupplier());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SupplierModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var supplier = await _context.Database.CreateSupplier();
        supplier.Name = "Updated supplier";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{supplier.ID}", supplier);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync(
            $"{Url}/{supplier.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated =
            await getResponse.Content.ReadFromJsonAsync<SupplierModel>();
        Assert.NotNull(updated);
        Assert.Equal("Updated supplier", updated.Name);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var supplier = ValidSupplier();
        supplier.ID = 999999;

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", supplier);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var supplier = await _context.Database.CreateSupplier();

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{supplier.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var response = await _context.Client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_ExistingSupplier_Returns200()
    {
        var supplier = await _context.Database.CreateSupplier();
        var item = await _context.Database.CreateItem();

        var response = await _context.Client.GetAsync(
            $"{Url}/{supplier.ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ItemModel>>();
        Assert.NotNull(items);
        Assert.Contains(items, result => result.ID == item.ID);
    }

    [Fact]
    public async Task GetItems_UnknownSupplier_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static SupplierModel ValidSupplier() =>
        new()
        {
            Code = $"SUP-{Guid.NewGuid():N}",
            Name = "Test Supplier",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Phone_Number = "0612345678",
            Reference = $"REF-{Guid.NewGuid():N}"
        };

    public void Dispose() => _context.Dispose();
}
