namespace Project.DataAccess;

public static class SupplierAcces
{
    private static string _Table = "Supplier";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<SupplierModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<SupplierModel>>(Array.Empty<SupplierModel>());

    public static Task<SupplierModel?> GetByIdAsync(int id) =>
        Task.FromResult<SupplierModel?>(null);

    public static Task<SupplierModel> CreateAsync(SupplierModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, SupplierModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<ItemModel>> GetItemsAsync(int id) =>
        Task.FromResult<IEnumerable<ItemModel>>(Array.Empty<ItemModel>());

    public static bool ValidateById(long id) => false;

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                supplier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                reference TEXT NOT NULL,
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