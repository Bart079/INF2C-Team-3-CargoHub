namespace Project.DataAccess;

public static class TransferAcces
{
    private static string _Table = "Transfer";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<TransferModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<TransferModel>>(Array.Empty<TransferModel>());

    public static Task<TransferModel?> GetByIdAsync(int id) =>
        Task.FromResult<TransferModel?>(null);

    public static Task<TransferModel> CreateAsync(TransferModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, TransferModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id) =>
        Task.FromResult<IEnumerable<TransferItemModel>>(Array.Empty<TransferItemModel>());

    public static Task<bool> CommitAsync(int id) =>
        Task.FromResult(false);

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                transfer_id INTEGER PRIMARY KEY AUTOINCREMENT,
                reference TEXT NOT NULL,
                from_location_id INTEGER NOT NULL REFERENCES Location(location_id),
                to_location_id INTEGER NOT NULL REFERENCES Location(location_id),
                transfer_status TEXT NOT NULL,
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