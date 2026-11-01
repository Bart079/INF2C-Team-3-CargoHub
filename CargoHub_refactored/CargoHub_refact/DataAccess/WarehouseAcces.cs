namespace Project.DataAccess;

public static class WarehouseAcces
{
    private static string _Table = "Warehouse";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<WarehouseModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<WarehouseModel>>(Array.Empty<WarehouseModel>());

    public static Task<WarehouseModel?> GetByIdAsync(int id) =>
        Task.FromResult<WarehouseModel?>(null);

    public static Task<WarehouseModel> CreateAsync(WarehouseModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, WarehouseModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<LocationModel>> GetLocationsAsync(int id) =>
        Task.FromResult<IEnumerable<LocationModel>>(Array.Empty<LocationModel>());

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                warehouse_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                contact_phone TEXT NOT NULL,
                contact_email TEXT NOT NULL,
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