public class WarehouseServiceTests
{
    private readonly WarehouseService _service = new();

    private static WarehouseModel ValidWarehouse()
    {
        return new WarehouseModel
        {
            ID = 1,
            Code = "WH001",
            Name = "Test Warehouse",
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
    public async Task Create_EmptyCode_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Code = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Name = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_EmptyAddress_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Address = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_InvalidEmail_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Contact_Email = "ongeldig-emailadres";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_InvalidPhone_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Contact_Phone = "abc123";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_ShortPhone_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.Contact_Phone = "12345";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Update_MismatchedId_ThrowsArgumentException()
    {
        var model = ValidWarehouse();
        model.ID = 1;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(2, model));
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(0);
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(
            0, ValidWarehouse());

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(0);
        Assert.False(result);
    }

    [Fact]
    public async Task GetLocations_InvalidId_ReturnsEmpty()
    {
        var result = await _service.GetLocationsAsync(0);
        Assert.Empty(result);
    }
}
