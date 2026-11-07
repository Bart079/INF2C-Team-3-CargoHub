using Dapper;
using Project.DataAccess;

public class Transfer_itemAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public Transfer_itemAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetItems_ReturnsTransferItemsForTransfer()
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await TransferAcces.Create(new TransferModel
        {
            Reference = "TRANSFER-1",
            From_Location_ID = from.ID,
            To_Location_ID = to.ID,
            Transfer_Status = "scheduled"
        });

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("""
            INSERT INTO Transfer_item (transfer_id, item_id, amount)
            VALUES (@transferId, @itemId, 5);
            """,
            new { transferId = transfer.ID, itemId = item.ID },
            transaction);

        var items = await Transfer_itemAcces.GetItems(
            transfer.ID, connection, transaction);
        await transaction.CommitAsync();

        var result = Assert.Single(items);
        Assert.Equal(transfer.ID, result.Transfer_ID);
        Assert.Equal(item.ID, result.Item_ID);
        Assert.Equal(5m, result.Amount);
    }

    [Fact]
    public void CreateTable_CreatesTransferItemTable()
    {
        Transfer_itemAcces.DropTable(_database.Path);

        Transfer_itemAcces.CreateTable(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void ResetDatabase_RecreatesTransferItemTable()
    {
        Transfer_itemAcces.ResetDatabase(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void DropTable_RemovesTransferItemTable()
    {
        Transfer_itemAcces.DropTable(_database.Path);

        Assert.False(TableExists());
    }

    private bool TableExists()
    {
        using var connection = DatabaseConnection.CreateConnection(_database.Path);
        connection.Open();
        return connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Transfer_item');");
    }

    public void Dispose() => _database.Dispose();
}
