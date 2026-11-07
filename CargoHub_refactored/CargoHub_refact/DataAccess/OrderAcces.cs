namespace Project.DataAccess;

public static class OrderAcces
{
    private static string _Table = "\"Order\"";

    public static async Task<IEnumerable<OrderModel>> GetAll()
    {
        const string sql = """
            SELECT
                order_id AS ID,
                client_id AS Client_ID,
                order_date AS Order_Date,
                request_date AS Request_Date,
                reference AS Reference,
                customer_po_number AS Customer_PO_Number,
                order_status AS Order_Status,
                shipping_notes AS Shipping_Notes,
                warehouse_id AS Warehouse_ID,
                ship_to_client_id AS Ship_To_Client_ID,
                bill_to_client_id AS Bill_To_Client_ID,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM "Order";
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<OrderModel>(sql);
    }

    public static async Task<OrderModel?> GetById(int id)
    {
        const string sql = """
            SELECT
                order_id AS ID,
                client_id AS Client_ID,
                order_date AS Order_Date,
                request_date AS Request_Date,
                reference AS Reference,
                customer_po_number AS Customer_PO_Number,
                order_status AS Order_Status,
                shipping_notes AS Shipping_Notes,
                warehouse_id AS Warehouse_ID,
                ship_to_client_id AS Ship_To_Client_ID,
                bill_to_client_id AS Bill_To_Client_ID,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM "Order"
            WHERE order_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<OrderModel>(sql, new { id });
    }

    public static async Task<OrderModel> Create(OrderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            INSERT INTO "Order" (
                client_id, request_date, reference, customer_po_number,
                order_status, shipping_notes, warehouse_id,
                ship_to_client_id, bill_to_client_id, created_at, updated_at
            )
            VALUES (
                @Client_ID, @Request_Date, @Reference, @Customer_PO_Number,
                @Order_Status, @Shipping_Notes, @Warehouse_ID,
                @Ship_To_Client_ID, @Bill_To_Client_ID,
                CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );
            SELECT last_insert_rowid();
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var id = await connection.QuerySingleAsync<int>(sql, model);
        return await GetById(id)
            ?? throw new InvalidOperationException("Created order could not be retrieved.");
    }

    public static async Task<bool> Update(int id, OrderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            UPDATE "Order"
            SET client_id = @Client_ID,
                request_date = @Request_Date,
                reference = @Reference,
                customer_po_number = @Customer_PO_Number,
                order_status = @Order_Status,
                shipping_notes = @Shipping_Notes,
                warehouse_id = @Warehouse_ID,
                ship_to_client_id = @Ship_To_Client_ID,
                bill_to_client_id = @Bill_To_Client_ID,
                updated_at = CURRENT_TIMESTAMP
            WHERE order_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var parameters = new DynamicParameters(model);
        parameters.Add("id", id);
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public static async Task<bool> Delete(int id)
    {
        const string sql = """DELETE FROM "Order" WHERE order_id = @id;""";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }

    public static async Task<IEnumerable<OrderItemModel>> GetItems(int id)
    {
        const string sql = """
            SELECT
                order_item_id AS ID,
                order_id AS Order_ID,
                item_id AS Item_ID,
                amount AS Amount,
                unit_price AS Unit_Price
            FROM Order_item
            WHERE order_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<OrderItemModel>(sql, new { id });
    }

    public static async Task<bool> UpdateItems(int id, IEnumerable<OrderItemModel> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var orderItems = items.ToArray();

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        const string orderExistsSql = """
            SELECT EXISTS(
                SELECT 1 FROM "Order" WHERE order_id = @id
            );
            """;
        var orderExists = await connection.QuerySingleAsync<bool>(
            orderExistsSql, new { id }, transaction);

        if (!orderExists)
            return false;

        const string deleteSql = "DELETE FROM Order_item WHERE order_id = @id;";
        await connection.ExecuteAsync(deleteSql, new { id }, transaction);

        const string insertSql = """
            INSERT INTO Order_item (order_id, item_id, amount, unit_price)
            VALUES (@Order_ID, @Item_ID, @Amount, @Unit_Price);
            """;

        foreach (var item in orderItems)
        {
            ArgumentNullException.ThrowIfNull(item);
            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    Order_ID = id,
                    item.Item_ID,
                    item.Amount,
                    item.Unit_Price
                },
                transaction);
        }

        await transaction.CommitAsync();
        return true;
    }

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                order_id INTEGER PRIMARY KEY AUTOINCREMENT,
                client_id INTEGER NOT NULL REFERENCES Client(client_id),
                order_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                request_date DATETIME NOT NULL,
                reference TEXT NOT NULL,
                customer_po_number TEXT NOT NULL,
                order_status TEXT NOT NULL,
                shipping_notes TEXT ,
                warehouse_id INTEGER NOT NULL REFERENCES Warehouse(warehouse_id),
                ship_to_client_id INTEGER NOT NULL REFERENCES Client(client_id),
                bill_to_client_id INTEGER NOT NULL REFERENCES Client(client_id),
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


    public static async Task<IEnumerable<OrderModel>> GetByClientId(int clientId)
    {
        const string sql = @"
            SELECT
                order_id AS ID,
                client_id AS Client_ID,
                order_date AS Order_Date,
                request_date AS Request_Date,
                reference AS Reference,
                customer_po_number AS Customer_PO_Number,
                order_status AS Order_Status,
                shipping_notes AS Shipping_Notes,
                warehouse_id AS Warehouse_ID,
                ship_to_client_id AS Ship_To_Client_ID,
                bill_to_client_id AS Bill_To_Client_ID,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM ""Order""
            WHERE client_id = @clientId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<OrderModel>(
            sql, new { clientId });
    }

    public static async Task<bool> ValidateId(int id)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM \"Order\" WHERE order_id = @id) AS IsValid";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { id });
    }
}