public class SupplierServiceTests
{
    private readonly SupplierService _service = new();

    private SupplierModel ValidSupplier()
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
}