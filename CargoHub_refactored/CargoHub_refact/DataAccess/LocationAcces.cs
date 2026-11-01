namespace Project.DataAccess;

public static class LocationAcces
{
    private static string _Table = "Location";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<LocationModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<LocationModel>>(Array.Empty<LocationModel>());

    public static Task<LocationModel?> GetByIdAsync(int id) =>
        Task.FromResult<LocationModel?>(null);

    public static Task<LocationModel> CreateAsync(LocationModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, LocationModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static bool ValidatedById(int id) => false;

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                location_id INTEGER PRIMARY KEY AUTOINCREMENT,
                warehouse_id INTEGER NOT NULL REFERENCES Warehouse(warehouse_id),
                code TEXT NOT NULL,
                name TEXT NOT NULL,
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