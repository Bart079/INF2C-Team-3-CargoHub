using Project.DataAccess;

namespace CargoHub.Tests.DataAcces;

[Collection("ShipmentAcces")]
public class Shipment_itemAccesTests
{
    [Fact]
    public void CreateTable_CreatesShipmentItemTable()
    {
        using var database = new TestDatabase();
        database.InitializePrerequisites();

        Shipment_itemAcces.CreateTable(database.Path);

        Assert.True(database.TableExists("Shipment_item"));
    }

    [Fact]
    public void DropTable_RemovesShipmentItemTable()
    {
        using var database = new TestDatabase();
        database.Initialize();

        Shipment_itemAcces.DropTable(database.Path);

        Assert.False(database.TableExists("Shipment_item"));
    }

    [Fact]
    public async Task ResetDatabase_CreatesEmptyShipmentItemTable()
    {
        using var database = new TestDatabase();
        database.Initialize();
        database.AddShipmentItem(1, 1, 3);

        Shipment_itemAcces.ResetDatabase(database.Path);

        Assert.True(database.TableExists("Shipment_item"));
        Assert.Empty(await Shipment_itemAcces.GetItemsByShipmentId(1));
    }

    [Fact]
    public async Task GetItemsByShipmentId_ReturnsOnlyItemsForRequestedShipment()
    {
        using var database = new TestDatabase();
        database.Initialize();
        database.AddShipmentItem(1, 1, 4);
        database.AddShipmentItem(1, 2, 6);
        database.AddShipmentItem(2, 3, 8);

        var items = (await Shipment_itemAcces.GetItemsByShipmentId(1)).ToList();

        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.Item_ID == 1 && item.Amount == 4);
        Assert.Contains(items, item => item.Item_ID == 2 && item.Amount == 6);
        Assert.All(items, item => Assert.Equal(1, item.Shipment_ID));
    }

    [Fact]
    public async Task ReplaceItemsForShipment_ReplacesOnlyRequestedShipmentsItems()
    {
        using var database = new TestDatabase();
        database.Initialize();
        database.AddShipmentItem(1, 1, 4);
        database.AddShipmentItem(2, 3, 8);

        await Shipment_itemAcces.ReplaceItemsForShipment(
            1,
            new[]
            {
                new ShipmentItemModel(0, 1, 2, 5),
                new ShipmentItemModel(0, 1, 3, 7)
            });

        var shipmentOneItems =
            (await Shipment_itemAcces.GetItemsByShipmentId(1)).ToList();
        var shipmentTwoItems =
            (await Shipment_itemAcces.GetItemsByShipmentId(2)).ToList();

        Assert.Equal(2, shipmentOneItems.Count);
        Assert.Contains(shipmentOneItems, item => item.Item_ID == 2 && item.Amount == 5);
        Assert.Contains(shipmentOneItems, item => item.Item_ID == 3 && item.Amount == 7);
        Assert.Single(shipmentTwoItems);
        Assert.Equal(3, shipmentTwoItems[0].Item_ID);
        Assert.Equal(8, shipmentTwoItems[0].Amount);
    }

    [Fact]
    public async Task ReplaceItemsForShipment_WithEmptyItems_RemovesExistingItems()
    {
        using var database = new TestDatabase();
        database.Initialize();
        database.AddShipmentItem(1, 1, 4);

        await Shipment_itemAcces.ReplaceItemsForShipment(
            1,
            Array.Empty<ShipmentItemModel>());

        Assert.Empty(await Shipment_itemAcces.GetItemsByShipmentId(1));
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string? _previousPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"shipment-item-access-tests-{Guid.NewGuid():N}.db");

        public TestDatabase()
        {
            using (File.Create(Path))
            {
            }

            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", Path);
        }

        public void InitializePrerequisites()
        {
            using var connection = DatabaseConnection.CreateConnection(Path);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE "Order" (order_id INTEGER PRIMARY KEY);
                CREATE TABLE Item (item_id INTEGER PRIMARY KEY);
                CREATE TABLE Shipment (
                    shipment_id INTEGER PRIMARY KEY,
                    order_id INTEGER NOT NULL REFERENCES "Order"(order_id)
                );
                INSERT INTO "Order" (order_id) VALUES (1), (2);
                INSERT INTO Item (item_id) VALUES (1), (2), (3);
                INSERT INTO Shipment (shipment_id, order_id) VALUES (1, 1), (2, 2);
                """;
            command.ExecuteNonQuery();
        }

        public void Initialize()
        {
            InitializePrerequisites();
            Shipment_itemAcces.CreateTable(Path);
        }

        public void AddShipmentItem(int shipmentId, int itemId, int amount)
        {
            using var connection = DatabaseConnection.CreateConnection(Path);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Shipment_item (shipment_id, item_id, amount)
                VALUES ($shipmentId, $itemId, $amount);
                """;
            command.Parameters.AddWithValue("$shipmentId", shipmentId);
            command.Parameters.AddWithValue("$itemId", itemId);
            command.Parameters.AddWithValue("$amount", amount);
            command.ExecuteNonQuery();
        }

        public bool TableExists(string tableName)
        {
            using var connection = DatabaseConnection.CreateConnection(Path);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT EXISTS(
                    SELECT 1
                    FROM sqlite_master
                    WHERE type = 'table' AND name = $tableName
                );
                """;
            command.Parameters.AddWithValue("$tableName", tableName);
            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", _previousPath);
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }
}