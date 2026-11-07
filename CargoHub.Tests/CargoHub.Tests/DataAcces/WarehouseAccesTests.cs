
using Dapper;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

public class WarehouseAccesTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string? _previousDbPath;

    public WarehouseAccesTests()
    {
        _previousDbPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"cargohub-warehouse-{Guid.NewGuid():N}.db"
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

        connection.Execute("""
            CREATE TABLE Warehouse (
                warehouse_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                contact_phone TEXT NOT NULL,
                contact_email TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE Location (
                location_id INTEGER PRIMARY KEY AUTOINCREMENT,
                warehouse_id INTEGER NOT NULL,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (warehouse_id)
                    REFERENCES Warehouse(warehouse_id)
            );
            """);
    }

    private static WarehouseModel ValidWarehouse()
    {
        return new WarehouseModel
        {
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
    public async Task GetAll_ReturnsWarehouses()
    {
        await WarehouseAcces.Create(ValidWarehouse());

        var warehouses = await WarehouseAcces.GetAll();

        Assert.Single(warehouses);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsWarehouse()
    {
        var created = await WarehouseAcces.Create(
            ValidWarehouse());

        var warehouse = await WarehouseAcces.GetById(created.ID);

        Assert.NotNull(warehouse);
        Assert.Equal(created.ID, warehouse.ID);
        Assert.Equal("Test Warehouse", warehouse.Name);
    }

    [Fact]
    public async Task GetById_Unknown_ReturnsNull()
    {
        var warehouse = await WarehouseAcces.GetById(999999);

        Assert.Null(warehouse);
    }

    [Fact]
    public async Task Create_AddsWarehouseToDatabase()
    {
        var model = ValidWarehouse();

        var created = await WarehouseAcces.Create(model);

        Assert.True(created.ID > 0);

        var stored = await WarehouseAcces.GetById(created.ID);

        Assert.NotNull(stored);
        Assert.Equal(model.Code, stored.Code);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Contact_Email, stored.Contact_Email);
    }

    [Fact]
    public async Task Update_ChangesWarehouse()
    {
        var warehouse = await WarehouseAcces.Create(
            ValidWarehouse());

        warehouse.Name = "Gewijzigd Warehouse";

        var updated = await WarehouseAcces.Update(
            warehouse.ID, warehouse);

        Assert.True(updated);

        var stored = await WarehouseAcces.GetById(warehouse.ID);

        Assert.NotNull(stored);
        Assert.Equal("Gewijzigd Warehouse", stored.Name);
    }

    [Fact]
    public async Task Update_Unknown_ReturnsFalse()
    {
        var updated = await WarehouseAcces.Update(
            999999, ValidWarehouse());

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesWarehouse()
    {
        var warehouse = await WarehouseAcces.Create(
            ValidWarehouse());

        var deleted = await WarehouseAcces.Delete(warehouse.ID);

        Assert.True(deleted);

        var stored = await WarehouseAcces.GetById(warehouse.ID);

        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_Unknown_ReturnsFalse()
    {
        var deleted = await WarehouseAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetLocations_ReturnsWarehouseLocations()
    {
        var warehouse = await WarehouseAcces.Create(
            ValidWarehouse());

        using (var connection = DatabaseConnection.CreateConnection())
        {
            await connection.OpenAsync();

            await connection.ExecuteAsync("""
                INSERT INTO Location (warehouse_id, code, name)
                VALUES (@id, 'LOC001', 'Test Location');
                """, new { id = warehouse.ID });
        }

        var locations = (await LocationAcces.GetWarehousesLocations(
            warehouse.ID)).ToList();

        Assert.Single(locations);
    }

    [Fact]
    public async Task Delete_WithLocations_ThrowsSqliteException()
    {
        var warehouse = await WarehouseAcces.Create(
            ValidWarehouse());

        using (var connection = DatabaseConnection.CreateConnection())
        {
            await connection.OpenAsync();

            await connection.ExecuteAsync("""
                INSERT INTO Location (warehouse_id, code, name)
                VALUES (@id, 'LOC001', 'Test Location');
                """, new { id = warehouse.ID });
        }

        var exception = await Assert.ThrowsAsync<SqliteException>(
            () => WarehouseAcces.Delete(warehouse.ID));

        Assert.Equal(787, exception.SqliteExtendedErrorCode);

        var stored = await WarehouseAcces.GetById(warehouse.ID);
        Assert.NotNull(stored);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
