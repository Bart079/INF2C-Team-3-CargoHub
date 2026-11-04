public class ClientServiceTests
{
    private readonly ClientService _service = new();

    private static ClientModel ValidClient()
    {
        return new ClientModel
        {
            Name = "Test Client",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Contact_Phone = "0612345678",
            Contact_Email = "test@example.com"
        };
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.Name = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(client));
    }

    [Fact]
    public async Task Create_InvalidEmail_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.Contact_Email = "ongeldig-emailadres";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(client));
    }

    [Fact]
    public async Task Create_InvalidPhone_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.Contact_Phone = "abc123";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(client));
    }

    [Fact]
    public async Task Create_ShortPhone_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.Contact_Phone = "123";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(client));
    }

    [Fact]
    public async Task Create_EmptyAddress_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.Address = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(client));
    }

    [Fact]
    public async Task Update_InvalidEmail_ThrowsArgumentException()
    {
        var client = ValidClient();
        client.ID = 1;
        client.Contact_Email = "ongeldig";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(1, client));
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, ValidClient());

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetOrders_InvalidId_ReturnsEmpty()
    {
        var result = await _service.GetOrdersAsync(-1);

        Assert.Empty(result);
    }
}
