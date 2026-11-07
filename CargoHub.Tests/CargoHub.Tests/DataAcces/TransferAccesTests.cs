using Dapper;
using Project.DataAccess;

namespace CargoHub.Tests.DataAcces;

[Collection("TransferAcces")]
public class TransferAccesTests
{
    [Fact]
    public async Task TransferMethods_ReadCreateUpdateAndDeleteTransfers()
    {
        using var database = new TestDatabase();
        database.Initialize();

        var created = await TransferAcces.Create(CreateTransfer());
        var saved = await TransferAcces.GetById(created.ID);
        var all = await TransferAcces.GetAll();

        Assert.NotNull(saved);
        Assert.Equal(created.ID, saved.ID);
        Assert.Equal("test-transfer", saved.Reference);
        Assert.Contains(all, transfer => transfer.ID == created.ID);

        var updatedTransfer = CreateTransfer("updated-transfer", "processed");
        Assert.True(await TransferAcces.Update(created.ID, updatedTransfer));

        saved = await TransferAcces.GetById(created.ID);
        Assert.NotNull(saved);
        Assert.Equal(updatedTransfer.Reference, saved.Reference);
        Assert.Equal(updatedTransfer.Transfer_Status, saved.Transfer_Status);

        Assert.True(await TransferAcces.Delete(created.ID));
        Assert.Null(await TransferAcces.GetById(created.ID));
    }

    [Fact]
    public async Task GetItems_ReturnsItemsForTransfer()
    {
        using var database = new TestDatabase();
        database.Initialize();
        var transfer = await TransferAcces.Create(CreateTransfer());

        using (var connection = DatabaseConnection.CreateConnection(database.Path))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Transfer_item (transfer_id, item_id, amount)
                VALUES (@transferId, 17, 3.5);";
            command.Parameters.AddWithValue("@transferId", transfer.ID);
            command.ExecuteNonQuery();
        }

        var items = (await TransferAcces.GetItems(transfer.ID)).ToList();

        var item = Assert.Single(items);
        Assert.Equal(transfer.ID, item.Transfer_ID);
        Assert.Equal(17, item.Item_ID);
        Assert.Equal(3.5m, item.Amount);
    }

    
    private static void AddTransferItem(string path, int transferId, decimal amount)
    {
        using var connection = DatabaseConnection.CreateConnection(path);
        connection.Open();
        connection.Execute(
            @"INSERT INTO Transfer_item (transfer_id, item_id, amount)
              VALUES (@transferId, 17, @amount);",
            new { transferId, amount });
    }

    private static void AddInventory(
        string path,
        int itemId,
        int locationId,
        int quantity)
    {
        using var connection = DatabaseConnection.CreateConnection(path);
        connection.Open();
        connection.Execute(
            @"INSERT INTO Inventory (
                  item_id, location_id, quantity_on_hand,
                  quantity_expected, quantity_ordered, quantity_allocated
              )
              VALUES (@itemId, @locationId, @quantity, 0, 0, 0);",
            new { itemId, locationId, quantity });
    }

    private static TransferModel CreateTransfer(
        string reference = "test-transfer",
        string status = "scheduled")
    {
        return new TransferModel
        {
            Reference = reference,
            From_Location_ID = 1,
            To_Location_ID = 2,
            Transfer_Status = status
        };
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string? _previousPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"transfer-tests-{Guid.NewGuid():N}.db");

        public TestDatabase()
        {
            using (File.Create(Path))
            {
            }

            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", Path);
        }

        public void Initialize()
        {
            using (var connection = DatabaseConnection.CreateConnection(Path))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE Location (location_id INTEGER PRIMARY KEY);
                    CREATE TABLE Item (item_id INTEGER PRIMARY KEY);
                    INSERT INTO Location (location_id) VALUES (1), (2);
                    INSERT INTO Item (item_id) VALUES (17);";
                command.ExecuteNonQuery();
            }

            TransferAcces.CreateTable(Path);
            Transfer_itemAcces.CreateTable(Path);
            InventoryAcces.CreateTable(Path);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", _previousPath);
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}

[CollectionDefinition("TransferAcces", DisableParallelization = true)]
public sealed class TransferAccesCollection
{
}
