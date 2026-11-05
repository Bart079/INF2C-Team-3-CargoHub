using System.Runtime.InteropServices;
using Dapper;
using Project.DataAccess;
using Microsoft.Data.Sqlite;

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
public async Task GetAll_noinventory_retursEmptyCollection()
{
    //Act
    var inventories = await InventoryAcces.GetAll();
    //Assert
    Assert.Empty(inventories); // controleert of er geen dingen terug komen
}

[Fact]
public async Task GetById_ReturnsInventory()
    {
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        var inventory_by_id = await InventoryAcces.GetById(created.ID);
        Assert.NotNull(inventory_by_id);
        Assert.Equal(created.ID, inventory_by_id.ID);
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
public async Task Create_InvalidForeignKey_ThrowsException()
    {
        //Arrange
        var model = ValidInventory();
        model.Item_ID = 999999; //ongeldige foreign key
        //Act + assert
        var exception = await Assert.ThrowsAsync<SqliteException>(
            () => InventoryAcces.Create(model));
    }

[Fact]
public async Task Update_ChangesInventory()
    {
        //Arrange
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        model.Quantity_On_Hand = 30;
        //Act
        var updated = await InventoryAcces.Update(created.ID, model);
        //Assert
        Assert.True(updated);
        var stored = await InventoryAcces.GetById(created.ID);
        Assert.NotNull(stored);
        Assert.Equal(model.Quantity_On_Hand, stored.Quantity_On_Hand);
    }

[Fact]
public async Task UpdateInventory_returnsfalse()
    {
        //Arrange
        var model = ValidInventory();
        int unknownId = 999999;
        //Act
        var update = await InventoryAcces.Update(unknownId, model);
        //Assert
        Assert.False(update);
    }

[Fact]
public async Task Delete_IDnotfound_returnsFalse()
    {
        //Arrange
        int unknownId = 999999;
        //Act
        var delete = await InventoryAcces.Delete(unknownId);
        //Assert
        Assert.False(delete);
    }

[Fact]
public async Task Delete_idfound_returnsTrue()
    {
        //Arrange
        var model = ValidInventory();
        var created = await InventoryAcces.Create(model);
        int idfound = created.ID;
        //Act
        var delete = await InventoryAcces.Delete(idfound);
        //Assert
        Assert.True(delete);
        var stored = await InventoryAcces.GetById(created.ID);
        Assert.Null(stored);
    }

[Fact] //data integriteit testt
public async Task ForeignKeys_AreEnabled()
{
    // Arrange maak verbinding met database, opent verbinding
    using var connection = DatabaseConnection.CreateConnection(); //using zorgt voor dat verbinding na de test wordt opgeruimd
    await connection.OpenAsync();

    // Act
    var foreignKeys = await connection.ExecuteScalarAsync<int>(
        "PRAGMA foreign_keys;"
    );

    // Assert
    Assert.Equal(1, foreignKeys);
}

    public void Dispose() //database opruimen
    {
        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

}