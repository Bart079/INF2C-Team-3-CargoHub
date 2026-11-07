public class SupplierServiceTests : IDisposable
{
    private readonly SupplierService _service = new();
    private readonly AccessTestDatabase _database = new();

    public SupplierServiceTests() => _database.CreateSchema();

    private static SupplierModel ValidSupplier()
    {
        return new SupplierModel
        {
            Code = "TEST001",
            Name = "Test Supplier",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Phone_Number = "0612345678",
            Reference = "TEST"
        };
    }

    [Fact]
    public async Task Create_InvalidPhone_ThrowsArgumentException()
    {
        // Arrange
        var supplier = ValidSupplier();
        supplier.Phone_Number = "abc123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(supplier));
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var supplier = ValidSupplier();
        supplier.Name = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(supplier));
    }

    [Fact]
    public async Task Create_NullModel_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(SupplierModel.Name))]
    [InlineData(nameof(SupplierModel.Address))]
    [InlineData(nameof(SupplierModel.City))]
    [InlineData(nameof(SupplierModel.Zip_Code))]
    [InlineData(nameof(SupplierModel.Country))]
    [InlineData(nameof(SupplierModel.Province))]
    [InlineData(nameof(SupplierModel.Contact_Name))]
    [InlineData(nameof(SupplierModel.Phone_Number))]
    [InlineData(nameof(SupplierModel.Code))]
    public async Task Create_EmptyRequiredField_ThrowsArgumentException(string property)
    {
        var supplier = ValidSupplier();
        switch (property)
        {
            case nameof(SupplierModel.Name): supplier.Name = ""; break;
            case nameof(SupplierModel.Address): supplier.Address = ""; break;
            case nameof(SupplierModel.City): supplier.City = ""; break;
            case nameof(SupplierModel.Zip_Code): supplier.Zip_Code = ""; break;
            case nameof(SupplierModel.Country): supplier.Country = ""; break;
            case nameof(SupplierModel.Province): supplier.Province = ""; break;
            case nameof(SupplierModel.Contact_Name): supplier.Contact_Name = ""; break;
            case nameof(SupplierModel.Phone_Number): supplier.Phone_Number = ""; break;
            case nameof(SupplierModel.Code): supplier.Code = ""; break;
        }

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(supplier));
    }

    [Fact]
    public async Task Create_ShortPhone_ThrowsArgumentException()
    {
        var supplier = ValidSupplier();
        supplier.Phone_Number = "12345";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(supplier));
    }

    [Fact]
    public async Task Create_ValidSupplier_PersistsSupplier()
    {
        var created = await _service.CreateAsync(ValidSupplier());
        var saved = await _service.GetByIdAsync(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(saved);
        Assert.Equal(created.Name, saved.Name);
    }

    [Fact]
    public async Task Update_MismatchedId_ThrowsArgumentException()
    {
        var supplier = ValidSupplier();
        supplier.ID = 1;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(2, supplier));
    }

    [Fact]
    public async Task Update_MatchingId_UpdatesSupplier()
    {
        var created = await _service.CreateAsync(ValidSupplier());
        created.Name = "Updated Supplier";

        var updated = await _service.UpdateAsync(created.ID, created);
        var saved = await _service.GetByIdAsync(created.ID);

        Assert.True(updated);
        Assert.NotNull(saved);
        Assert.Equal("Updated Supplier", saved.Name);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var updated = await _service.UpdateAsync(0, null!);

        Assert.False(updated);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(0);

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(0);

        Assert.False(result);
    }

    [Fact]
    public async Task GetSupplierItems_InvalidId_ReturnsEmpty()
    {
        var result = await _service.GetSupplierItemsAsync(0);

        Assert.Empty(result);
    }

    public void Dispose() => _database.Dispose();
}