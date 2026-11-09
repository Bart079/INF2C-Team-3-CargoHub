using System.Net;
using System.Net.Http.Json;

public class TransfersApiTests : IDisposable
{
    private const string Url = "/api/v1/transfers";
    private readonly ApiTestContext _context = new();

    [Fact]
    public async Task GetAll_Returns200()
    {
        var transfer = await _context.Database.CreateTransfer();

        var response = await _context.Client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transfers =
            await response.Content.ReadFromJsonAsync<List<TransferModel>>();
        Assert.NotNull(transfers);
        Assert.Contains(transfers, result => result.ID == transfer.ID);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var transfer = await _context.Database.CreateTransfer();

        var response = await _context.Client.GetAsync(
            $"{Url}/{transfer.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TransferModel>();
        Assert.NotNull(result);
        Assert.Equal(transfer.ID, result.ID);
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
        var model = await CreateValidTransfer();

        var response = await _context.Client.PostAsJsonAsync(Url, model);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TransferModel>();
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var transfer = await _context.Database.CreateTransfer();
        transfer.Reference = "TRANSFER-UPDATED";

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/{transfer.ID}", transfer);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _context.Client.GetAsync(
            $"{Url}/{transfer.ID}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var updated = await getResponse.Content.ReadFromJsonAsync<TransferModel>();
        Assert.NotNull(updated);
        Assert.Equal("TRANSFER-UPDATED", updated.Reference);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var model = await CreateValidTransfer();

        var response = await _context.Client.PutAsJsonAsync(
            $"{Url}/999999", model);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var transfer = await _context.Database.CreateTransfer();

        var response = await _context.Client.DeleteAsync(
            $"{Url}/{transfer.ID}");

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
        var transfer = await _context.Database.CreateTransfer();

        var response = await _context.Client.GetAsync(
            $"{Url}/{transfer.ID}/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(
            await response.Content.ReadFromJsonAsync<List<TransferItemModel>>());
    }

    [Fact]
    public async Task GetItems_Unknown_Returns404()
    {
        var response = await _context.Client.GetAsync(
            $"{Url}/999999/items");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Commit_Successful_Returns204()
    {
        var transfer = await _context.Database.CreateTransfer();
        var item = await _context.Database.CreateItem();
        var sourceLocation = await Project.DataAccess.LocationAcces.GetById(
            transfer.From_Location_ID);
        Assert.NotNull(sourceLocation);
        await _context.Database.CreateInventory(item, sourceLocation);
        await _context.Database.AddTransferItem(
            transfer.ID, checked((int)item.ID), 4);

        var response = await _context.Client.PostAsync(
            $"{Url}/{transfer.ID}/commit", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Commit_Unsuccessful_Returns404()
    {
        var response = await _context.Client.PostAsync(
            $"{Url}/999999/commit", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<TransferModel> CreateValidTransfer()
    {
        var transfer = await _context.Database.CreateTransfer();
        transfer.ID = 0;
        transfer.Reference = $"TRANSFER-{Guid.NewGuid():N}";
        return transfer;
    }

    public void Dispose() => _context.Dispose();
}
