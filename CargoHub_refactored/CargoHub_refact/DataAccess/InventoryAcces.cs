namespace Project.DataAccess;

public static class InventoryAcces
{
    private static string _Table = "Inventory";

    public static async Task<InventoryTotalsModel> GetInventoryTotalsAsync(long itemId)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT
                @itemId AS Item_ID,
                COALESCE(SUM(quantity_on_hand), 0) AS Total_Quantity_On_Hand,
                COALESCE(SUM(quantity_expected), 0) AS Total_Quantity_Expected,
                COALESCE(SUM(quantity_ordered), 0) AS Total_Quantity_Ordered,
                COALESCE(SUM(quantity_allocated), 0) AS Total_Quantity_Allocated
            FROM Inventory
            WHERE item_id = @itemId;
            """;

        return await connection.QuerySingleAsync<InventoryTotalsModel>(sql, new { itemId });
    }

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                inventory_id INTEGER PRIMARY KEY AUTOINCREMENT,
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                location_id INTEGER NOT NULL REFERENCES Location(location_id),
                quantity_on_hand INTEGER NOT NULL,
                quantity_expected INTEGER NOT NULL,
                quantity_ordered INTEGER NOT NULL,
                quantity_allocated INTEGER NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
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
}