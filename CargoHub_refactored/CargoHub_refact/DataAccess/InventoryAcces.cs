namespace Project.DataAccess;

public static class InventoryAcces
{
    private static string _Table = "Inventory";

    public static async Task<InventoryTotalsModel> GetInventoryTotals(long itemId)
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
    public static async Task<IEnumerable<InventoryModel>> GetAll()
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                inventory_id AS ID,
                item_id AS Item_ID,
                location_id AS Location_ID,
                quantity_on_hand AS Quantity_On_Hand,
                quantity_expected AS Quantity_Expected,
                quantity_ordered AS Quantity_Ordered,
                quantity_allocated AS Quantity_Allocated,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Inventory;";
        return await connection.QueryAsync<InventoryModel>(sql);
    }
    public static async Task<InventoryModel?> GetById(int id)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                inventory_id AS ID,
                item_id AS Item_ID,
                location_id AS Location_ID,
                quantity_on_hand AS Quantity_On_Hand,
                quantity_expected AS Quantity_Expected,
                quantity_ordered AS Quantity_Ordered,
                quantity_allocated AS Quantity_Allocated,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Inventory
            WHERE inventory_id = @id;";
        return await connection.QuerySingleOrDefaultAsync<InventoryModel>(sql, new { id });
    }

    public static async Task<InventoryModel> Create(InventoryModel model)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string insertSql = @"
            INSERT INTO Inventory (item_id, location_id, quantity_on_hand, quantity_expected, quantity_ordered, quantity_allocated)
            VALUES (@Item_ID, @Location_ID, @Quantity_On_Hand, @Quantity_Expected, @Quantity_Ordered, @Quantity_Allocated);
            SELECT last_insert_rowid();";

        var id = await connection.ExecuteScalarAsync<long>(insertSql, model);

        const string selectSql = @"
            SELECT
                inventory_id AS ID,
                item_id AS Item_ID,
                location_id AS Location_ID,
                quantity_on_hand AS Quantity_On_Hand,
                quantity_expected AS Quantity_Expected,
                quantity_ordered AS Quantity_Ordered,
                quantity_allocated AS Quantity_Allocated,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Inventory
            WHERE inventory_id = @id;";

        return await connection.QuerySingleAsync<InventoryModel>(selectSql, new { id });
    }
    public static async Task<bool> Update(int id, InventoryModel model)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Inventory
            SET item_id = @Item_ID,
                location_id = @Location_ID,
                quantity_on_hand = @Quantity_On_Hand,
                quantity_expected = @Quantity_Expected,
                quantity_ordered = @Quantity_Ordered,
                quantity_allocated = @Quantity_Allocated,
                updated_at = CURRENT_TIMESTAMP
            WHERE inventory_id = @id";

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            id,
            model.Item_ID,
            model.Location_ID,
            model.Quantity_On_Hand,
            model.Quantity_Expected,
            model.Quantity_Ordered,
            model.Quantity_Allocated
        });
        return affectedRows > 0;
    }
    public static async Task<bool> Delete(int id)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        const string sql = "DELETE FROM Inventory WHERE inventory_id = @id";
        var affectedRows = await connection.ExecuteAsync(sql, new { id });
        return affectedRows > 0;
    }
}