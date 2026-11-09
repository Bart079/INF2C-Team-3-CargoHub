namespace Project.DataAccess;

public static class Item_groupAcces
{
    private static string _Table = "Item_group";

    public static async Task<IEnumerable<ItemGroupModel>> GetAll()
    {
        const string sql = """
            SELECT
                item_group_id AS ID,
                name AS Name,
                description AS Description,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Item_group;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<ItemGroupModel>(sql);
    }

    public static async Task<ItemGroupModel?> GetById(int id)
    {
        const string sql = """
            SELECT
                item_group_id AS ID,
                name AS Name,
                description AS Description,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Item_group
            WHERE item_group_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<ItemGroupModel>(sql, new { id });
    }

    public static async Task<ItemGroupModel> Create(ItemGroupModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            INSERT INTO Item_group (name, description, created_at, updated_at)
            VALUES (@Name, @Description, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            SELECT last_insert_rowid();
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var id = await connection.QuerySingleAsync<int>(sql, model);
        return await GetById(id)
            ?? throw new InvalidOperationException("Created item group could not be retrieved.");
    }

    public static async Task<bool> Update(int id, ItemGroupModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            UPDATE Item_group
            SET name = @Name,
                description = @Description,
                updated_at = CURRENT_TIMESTAMP
            WHERE item_group_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var parameters = new DynamicParameters(model);
        parameters.Add("id", id);
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public static async Task<bool> Delete(int id)
    {
        const string sql = "DELETE FROM Item_group WHERE item_group_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }

    public static async Task<IEnumerable<ItemModel>> GetItems(int id)
    {
        const string sql = """
            SELECT
                item_id AS ID,
                code AS Code,
                description AS Description,
                barcode AS Barcode,
                model_number AS Model_Number,
                commodity_code AS Commodity_Code,
                unit_weight AS Unit_Weight,
                item_line_id AS Item_Line_ID,
                item_group_id AS Item_Group_ID,
                item_type_id AS Item_Type_ID,
                min_purchase_quantity AS Min_Purchase_Quantity,
                case_size AS Case_Size,
                packaging_type AS Packaging_Type,
                order_multiple AS Order_Multiple,
                supplier_id AS Supplier_ID,
                supplier_sku AS Supplier_SKU,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Item
            WHERE item_group_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<ItemModel>(sql, new { id });
    }

    public static bool ValidateById(long id)
    {
        const string sql = """
            SELECT NOT EXISTS(
                SELECT 1 FROM Item_group WHERE item_group_id = @id
            );
            """;

        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        return connection.QuerySingle<bool>(sql, new { id });
    }

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                item_group_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT,
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