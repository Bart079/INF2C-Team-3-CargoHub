namespace Project.DataAccess;

public static class OrderAcces
{
    private static string _Table = "\"Order\"";

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
                shipping_notes TEXT NOT NULL,
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
}