namespace Project.DataAccess;

public static class OrderAcces
{
    private static string _Table = "\"Order\"";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<OrderModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<OrderModel>>(Array.Empty<OrderModel>());

    public static Task<OrderModel?> GetByIdAsync(int id) =>
        Task.FromResult<OrderModel?>(null);

    public static Task<OrderModel> CreateAsync(OrderModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, OrderModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<OrderItemModel>> GetItemsAsync(int id) =>
        Task.FromResult<IEnumerable<OrderItemModel>>(Array.Empty<OrderItemModel>());

    public static Task<bool> UpdateItemsAsync(int id, IEnumerable<OrderItemModel> items) =>
        Task.FromResult(false);

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