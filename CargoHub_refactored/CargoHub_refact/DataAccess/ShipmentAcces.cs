namespace Project.DataAccess;

public static class ShipmentAcces
{
    private static string _Table = "Shipment";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                shipment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_id INTEGER NOT NULL REFERENCES ""Order""(order_id),
                reference TEXT NOT NULL,
                shipment_date DATETIME NOT NULL,
                shipment_type TEXT NOT NULL,
                shipment_status TEXT NOT NULL,
                carrier_name TEXT NOT NULL,
                shipping_method TEXT NOT NULL,
                payment_type TEXT NOT NULL,
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

    public static async Task<IEnumerable<ShipmentModel>> GetAll()
    {
        const string sql = @"
            SELECT
                shipment_id AS ID,
                order_id AS Order_ID,
                reference AS Reference,
                shipment_date AS Shipment_Date,
                shipment_type AS Shipment_Type,
                shipment_status AS Shipment_Status,
                carrier_name AS Carrier_Name,
                shipping_method AS Shipping_Method,
                payment_type AS Payment_Type,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Shipment;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<ShipmentModel>(sql);
    }

    public static async Task<ShipmentModel?> GetById(int id)
    {
        const string sql = @"
            SELECT
                shipment_id AS ID,
                order_id AS Order_ID,
                reference AS Reference,
                shipment_date AS Shipment_Date,
                shipment_type AS Shipment_Type,
                shipment_status AS Shipment_Status,
                carrier_name AS Carrier_Name,
                shipping_method AS Shipping_Method,
                payment_type AS Payment_Type,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Shipment
            WHERE shipment_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<ShipmentModel>(sql, new { id });
    }

    public static async Task<ShipmentModel> Create(ShipmentModel model)
    {
        const string sql = @"
            INSERT INTO Shipment (
                order_id, reference, shipment_date, shipment_type,
                shipment_status, carrier_name, shipping_method, payment_type,
                created_at, updated_at
            )
            VALUES (
                @Order_ID, @Reference, @Shipment_Date, @Shipment_Type,
                @Shipment_Status, @Carrier_Name, @Shipping_Method, @Payment_Type,
                CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );

            SELECT last_insert_rowid();";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var id = await connection.QuerySingleAsync<int>(sql, model);

        return await GetById(id)
            ?? throw new InvalidOperationException(
                "Created shipment could not be retrieved.");
    }

    public static async Task<bool> Update(int shipmentId, ShipmentModel model)
    {
        const string sql = @"
            UPDATE Shipment
            SET order_id = @Order_ID,
                reference = @Reference,
                shipment_date = @Shipment_Date,
                shipment_type = @Shipment_Type,
                shipment_status = @Shipment_Status,
                carrier_name = @Carrier_Name,
                shipping_method = @Shipping_Method,
                payment_type = @Payment_Type,
                updated_at = CURRENT_TIMESTAMP
            WHERE shipment_id = @shipmentId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var parameters = new DynamicParameters(model);
        parameters.Add("shipmentId", shipmentId);

        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public static async Task<bool> Delete(int shipmentId)
    {
        const string sql = @"
            DELETE FROM Shipment
            WHERE shipment_id = @shipmentId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { shipmentId });

        return rowsAffected > 0;
    }

    public static async Task<IEnumerable<int>> GetOrders(int shipmentId)
    {
        const string sql = @"
            SELECT order_id
            FROM Shipment
            WHERE shipment_id = @shipmentId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<int>(sql, new { shipmentId });
    }

    public static async Task<bool> UpdateOrders(
        int shipmentId,
        IEnumerable<int> orderIds)
    {
        ArgumentNullException.ThrowIfNull(orderIds);

        var orderId = orderIds.FirstOrDefault(id => id > 0);
        if (orderId == 0)
        {
            return false;
        }

        const string sql = @"
            UPDATE Shipment
            SET order_id = @orderId,
                updated_at = CURRENT_TIMESTAMP
            WHERE shipment_id = @shipmentId;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(
            sql, new { shipmentId, orderId });
        return rowsAffected > 0;
    }

}
