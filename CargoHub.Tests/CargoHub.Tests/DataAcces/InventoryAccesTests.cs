using System.Runtime.InteropServices;
using Dapper;
using Project.DataAccess;

public class InventoryAccesTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string? _previousDbPath;

    public InventoryAccesTests()
    {
        // database klaarzetten
        _previousDbPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        // tijdelijke database
        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"inventory-test-{Guid.NewGuid():N}.db"
        );

        using (File.Create(_dbPath)) { }

        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _dbPath);

        CreateTestTables();
    }

    private void CreateTestTables() //database klaarzetten method
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();

        connection.Execute(@"
            CREATE TABLE Item (
                item_id INTEGER PRIMARY KEY
            );

            CREATE TABLE Location (
                location_id INTEGER PRIMARY KEY
            );

            INSERT INTO Item (item_id) VALUES (1);
            INSERT INTO Location (location_id) VALUES (1);
        ");

        InventoryAcces.CreateTable(_dbPath); //tabellen maken
    }

    private static InventoryModel ValidInventory() //testdata helper
        {
            return new InventoryModel
            {
                Item_ID = 1,
                Location_ID = 1,
                Quantity_On_Hand = 20,
                Quantity_Expected = 5,
                Quantity_Ordered = 10,
                Quantity_Allocated = 3
            };
        }
    private void CreateTestInventory()
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();

        connection.Execute(
            "INSERT INTO Item (item_id) VALUES (1);");
        
        connection.Execute(
            "INSERT INTO Location (location_id) VALUES (1);");
    }

//tests

[Fact]
public async Task GetAll_ReturnsInventory()
{
    // Arrange
    var model = ValidInventory();
    await InventoryAcces.Create(model);
    //Act
    var inventories = await InventoryAcces.GetAll();
    //Assert
    Assert.Single(inventories); // controleert of er 1 ding terug komt
}

[Fact]
public async Task GetById_ReturnsInventory()
    {
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        var inventory_by_id = await InventoryAcces.GetById(created.ID);
        Assert.NotNull(inventory_by_id);
        Assert.Equal(created.ID, inventory_by_id.Item_ID);
    }
[Fact]
public async Task GetById_UnknownInventory_ReturnsNull()
{
    int unknownId = 999999;
    var inventory = await InventoryAcces.GetById(unknownId);
    Assert.Null(inventory);
}
[Fact]
public async Task Create_AddsInventory()
    {
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.Equal(model.Item_ID, created.Item_ID);
        Assert.Equal(model.Location_ID, created.Location_ID);
        var stored = await InventoryAcces.GetById(created.ID);

        Assert.NotNull(stored);
        Assert.Equal(model.Item_ID, stored.Item_ID);
        Assert.Equal(model.Location_ID, stored.Location_ID);    
    }

[Fact]
public async Task Update_ChangesInventory()
    {
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        Assert.NotNull(created);
        Assert.True(created.ID > 0);
        Assert.Equal(created.ID, created.Item_ID);
        Assert.Equal(model.Location_ID, created.Location_ID);
        var stored = await InventoryAcces.GetById(created.ID);
    }

    public void Dispose() //database opruimen
    {
        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

}