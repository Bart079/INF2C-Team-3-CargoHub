using Dapper;
using Project.DataAccess;

public class Order_itemAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    [Fact]
    public void CreateTable_CreatesOrderItemTable()
    {
        CreatePrerequisiteTables();

        Order_itemAcces.CreateTable(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void ResetDatabase_RecreatesOrderItemTable()
    {
        CreatePrerequisiteTables();
        Order_itemAcces.CreateTable(_database.Path);

        Order_itemAcces.ResetDatabase(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void DropTable_RemovesOrderItemTable()
    {
        CreatePrerequisiteTables();
        Order_itemAcces.CreateTable(_database.Path);

        Order_itemAcces.DropTable(_database.Path);

        Assert.False(TableExists());
    }

    private void CreatePrerequisiteTables()
    {
        ClientAcces.CreateTable(_database.Path);
        WarehouseAcces.CreateTable(_database.Path);
        Item_groupAcces.CreateTable(_database.Path);
        Item_lineAcces.CreateTable(_database.Path);
        Item_typeAcces.CreateTable(_database.Path);
        SupplierAcces.CreateTable(_database.Path);
        ItemAcces.CreateTable(_database.Path);
        OrderAcces.CreateTable(_database.Path);
    }

    private bool TableExists()
    {
        using var connection = DatabaseConnection.CreateConnection(_database.Path);
        connection.Open();
        return connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Order_item');");
    }

    public void Dispose() => _database.Dispose();
}
