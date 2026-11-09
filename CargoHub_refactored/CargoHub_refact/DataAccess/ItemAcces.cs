namespace Project.DataAccess;

public static class ItemAcces
{
    private static string _Table = "Item";

    public static async Task<IEnumerable<ItemModel>> GetAll()
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
            FROM Item;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<ItemModel>(sql);
    }

    public static async Task<ItemModel?> GetById(long id)
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
            WHERE item_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<ItemModel>(sql, new { id });
    }

    public static async Task<ItemModel> Create(ItemModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            INSERT INTO Item (
                code, description, barcode, model_number, commodity_code,
                unit_weight, item_line_id, item_group_id, item_type_id,
                min_purchase_quantity, case_size, packaging_type,
                order_multiple, supplier_id, supplier_sku,
                created_at, updated_at
            )
            VALUES (
                @Code, @Description, @Barcode, @Model_Number, @Commodity_Code,
                @Unit_Weight, @Item_Line_ID, @Item_Group_ID, @Item_Type_ID,
                @Min_Purchase_Quantity, @Case_Size, @Packaging_Type,
                @Order_Multiple, @Supplier_ID, @Supplier_SKU,
                CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );
            SELECT last_insert_rowid();
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var id = await connection.QuerySingleAsync<long>(sql, model);
        return await GetById(id)
            ?? throw new InvalidOperationException("Created item could not be retrieved.");
    }

    public static async Task<bool> Update(long id, ItemModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            UPDATE Item
            SET code = @Code,
                description = @Description,
                barcode = @Barcode,
                model_number = @Model_Number,
                commodity_code = @Commodity_Code,
                unit_weight = @Unit_Weight,
                item_line_id = @Item_Line_ID,
                item_group_id = @Item_Group_ID,
                item_type_id = @Item_Type_ID,
                min_purchase_quantity = @Min_Purchase_Quantity,
                case_size = @Case_Size,
                packaging_type = @Packaging_Type,
                order_multiple = @Order_Multiple,
                supplier_id = @Supplier_ID,
                supplier_sku = @Supplier_SKU,
                updated_at = CURRENT_TIMESTAMP
            WHERE item_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var parameters = new DynamicParameters(model);
        parameters.Add("id", id);
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public static async Task<bool> Delete(long id)
    {
        const string sql = "DELETE FROM Item WHERE item_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                description TEXT NOT NULL,
                barcode TEXT NOT NULL,
                model_number TEXT NOT NULL,
                commodity_code INTEGER NOT NULL,
                unit_weight REAL NOT NULL,
                item_line_id INTEGER NOT NULL REFERENCES Item_line(item_line_id),
                item_group_id INTEGER NOT NULL REFERENCES Item_group(item_group_id),
                item_type_id INTEGER NOT NULL REFERENCES Item_type(item_type_id),
                min_purchase_quantity INTEGER NOT NULL,
                case_size INTEGER NOT NULL,
                packaging_type TEXT NOT NULL,
                order_multiple INTEGER NOT NULL,
                supplier_id INTEGER NOT NULL REFERENCES Supplier(supplier_id),
                supplier_sku TEXT NOT NULL,
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

    public static bool ValidatedById(int id)
    {
        const string sql = "SELECT NOT EXISTS(SELECT 1 FROM Item WHERE item_id = @id);";

        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        return connection.QuerySingle<bool>(sql, new { id });
    }

    public static async Task<bool> AllExist(IEnumerable<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var itemIds = ids.Distinct().ToArray();
        if (itemIds.Length == 0)
            return true;

        const string sql = """
            SELECT COUNT(*)
            FROM Item
            WHERE item_id IN @itemIds;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var foundCount = await connection.QuerySingleAsync<int>(sql, new { itemIds });
        return foundCount == itemIds.Length;
    }

    // supplier
    public static async Task<IEnumerable<ItemModel>> GetSupplierItemsById(int id)
    {
        const string sql = @"
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
            WHERE supplier_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<ItemModel>(sql, new { id });
    }
}