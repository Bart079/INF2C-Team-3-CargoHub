namespace Project.DataAccess;

public static class ShipmentAcces
{
    private static string _Table = "Shipment";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<ShipmentModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<ShipmentModel>>(Array.Empty<ShipmentModel>());

    public static Task<ShipmentModel?> GetByIdAsync(int id) =>
        Task.FromResult<ShipmentModel?>(null);

    public static Task<ShipmentModel> CreateAsync(ShipmentModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, ShipmentModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<int>> GetOrdersAsync(int id) =>
        Task.FromResult<IEnumerable<int>>(Array.Empty<int>());

    public static Task<IEnumerable<ShipmentItemModel>> GetItemsAsync(int id) =>
        Task.FromResult<IEnumerable<ShipmentItemModel>>(Array.Empty<ShipmentItemModel>());

    public static Task<bool> UpdateOrdersAsync(int id, IEnumerable<int> orderIds) =>
        Task.FromResult(false);

    public static Task<bool> UpdateItemsAsync(int id, IEnumerable<ShipmentItemModel> items) =>
        Task.FromResult(false);

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
}