namespace Project.DataAccess;

public static class Shipment_itemAcces
{
    private static string _Table = "Shipment_item";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                shipment_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                shipment_id INTEGER NOT NULL REFERENCES Shipment(shipment_id),
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                amount INTEGER NOT NULL
            );";

        using (var connection = DatabaseConnection.CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string path)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = DatabaseConnection.CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
            connection.Execute("VACUUM;");
        }
    }

    public static void ResetDatabase(string path)
    {
        DropTable(path);
        CreateTable(path);
    }
    public static async Task<IEnumerable<ShipmentItemModel>> GetItemsByShipmentId(int shipmentId)
    {
        const string sql = @"
            SELECT
                shipment_item_id AS ID,
                shipment_id AS Shipment_ID,
                item_id AS Item_ID,
                amount AS Amount
            FROM Shipment_item
            WHERE shipment_id = @shipmentId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<ShipmentItemModel>(
            sql, new { shipmentId });
    }
    public static async Task ReplaceItemsForShipment(
        int shipmentId,
        IEnumerable<ShipmentItemModel> items)
    {
        var replacementItems = items.ToArray();

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM Shipment_item WHERE shipment_id = @shipmentId;",
            new {  shipmentId },
            transaction);

        const string insertSql = """
            INSERT INTO Shipment_item (shipment_id, item_id, amount)
            VALUES (@shipmentId, @Item_ID, @Amount);
            """;

        await connection.ExecuteAsync(
            insertSql,
            replacementItems.Select(item => new
            {
                shipmentId,
                item.Item_ID,
                item.Amount
            }),
            transaction);

        await transaction.CommitAsync();
    }
}