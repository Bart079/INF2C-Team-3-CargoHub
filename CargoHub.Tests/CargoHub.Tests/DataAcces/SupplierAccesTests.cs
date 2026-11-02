
using Dapper;
using Microsoft.Data.Sqlite;
using Project.DataAccess;
using Xunit;

public class SupplierAccesTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string? _previousDbPath;

    public SupplierAccesTests()
    {
        // Bewaar eventueel bestaande configuratie
        _previousDbPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        // Maak een unieke tijdelijke database
        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"cargohub-test-{Guid.NewGuid():N}.db"
        );

        using (File.Create(_dbPath)) { }

        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _dbPath);

        CreateTestTables();
    }

    private void CreateTestTables()
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();

        connection.Execute(@"
            CREATE TABLE Supplier (
                supplier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                reference TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE Item (
                item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                description TEXT NOT NULL,
                barcode TEXT NOT NULL,
                model_number TEXT,
                commodity_code INTEGER,
                unit_weight REAL,
                item_line_id INTEGER,
                item_group_id INTEGER,
                item_type_id INTEGER,
                min_purchase_quantity INTEGER,
                case_size INTEGER,
                packaging_type TEXT,
                order_multiple INTEGER,
                supplier_id INTEGER,
                supplier_sku TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (supplier_id)
                    REFERENCES Supplier(supplier_id)
            );
        ");
    }

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
    public async Task GetAll_ReturnsSuppliers()
    {
        // Arrange
        await SupplierAcces.Create(ValidSupplier());

        // Act
        var suppliers = await SupplierAcces.GetAll();

        // Assert
        Assert.Single(suppliers);
    }

    [Fact]
    public async Task GetById_ExistingSupplier_ReturnsSupplier()
    {
        // Arrange
        var created = await SupplierAcces.Create(
            ValidSupplier());

        // Act
        var supplier = await SupplierAcces.GetById(created.ID);

        // Assert
        Assert.NotNull(supplier);
        Assert.Equal("Test Supplier", supplier.Name);
        Assert.Equal(created.ID, supplier.ID);
    }

    [Fact]
    public async Task GetById_UnknownSupplier_ReturnsNull()
    {
        // Act
        var supplier = await SupplierAcces.GetById(999999);

        // Assert
        Assert.Null(supplier);
    }

    [Fact]
    public async Task Create_AddsSupplierToDatabase()
    {
        // Arrange
        var model = ValidSupplier();

        // Act
        var created = await SupplierAcces.Create(model);

        // Assert
        Assert.True(created.ID > 0);

        var stored = await SupplierAcces.GetById(created.ID);

        Assert.NotNull(stored);
        Assert.Equal(model.Code, stored.Code);
        Assert.Equal(model.Name, stored.Name);
    }

    [Fact]
    public async Task Update_ChangesSupplier()
    {
        // Arrange
        var supplier = await SupplierAcces.Create(
            ValidSupplier());

        supplier.Name = "Gewijzigde Supplier";

        // Act
        var updated = await SupplierAcces.Update(
            supplier.ID, supplier);

        // Assert
        Assert.True(updated);

        var stored = await SupplierAcces.GetById(supplier.ID);

        Assert.NotNull(stored);
        Assert.Equal("Gewijzigde Supplier", stored.Name);
    }

    [Fact]
    public async Task Delete_RemovesSupplier()
    {
        // Arrange
        var supplier = await SupplierAcces.Create(
            ValidSupplier());

        // Act
        var deleted = await SupplierAcces.Delete(supplier.ID);

        // Assert
        Assert.True(deleted);

        var stored = await SupplierAcces.GetById(supplier.ID);

        Assert.Null(stored);
    }

    [Fact]
    public async Task GetItems_ReturnsSupplierItems()
    {
        // Arrange
        var supplier = await SupplierAcces.Create(
            ValidSupplier());

        using (var connection =
            DatabaseConnection.CreateConnection())
        {
            await connection.OpenAsync();

            await connection.ExecuteAsync(@"
                INSERT INTO Item (
                    code, description, barcode,
                    model_number, commodity_code,
                    unit_weight, item_line_id,
                    item_group_id, item_type_id,
                    min_purchase_quantity, case_size,
                    packaging_type, order_multiple,
                    supplier_id, supplier_sku
                )
                VALUES (
                    'ITEM001', 'Test Item', '123456789',
                    'MODEL1', 100, 2.5, 1,
                    1, 1, 10, 5,
                    'Box', 1, @supplierId, 'SKU001'
                );",
                new { supplierId = supplier.ID });
        }

        // Act
        var items = (await ItemAcces.GetSupplierItemsById(
            supplier.ID)).ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal("ITEM001", items[0].Code);
        Assert.Equal((long)supplier.ID, items[0].Supplier_ID);
    }

    public void Dispose()
    {
        // Zet oorspronkelijke configuratie terug
        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        // Verwijder uitsluitend onze tijdelijke database
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}


