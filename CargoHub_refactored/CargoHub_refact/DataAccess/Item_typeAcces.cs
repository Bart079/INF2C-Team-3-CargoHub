namespace Project.DataAccess;

public static class Item_typeAcces
{
    private static string _Table = "Item_type";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<ItemTypeModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<ItemTypeModel>>(Array.Empty<ItemTypeModel>());

    public static Task<ItemTypeModel?> GetByIdAsync(int id) =>
        Task.FromResult<ItemTypeModel?>(null);

    public static Task<ItemTypeModel> CreateAsync(ItemTypeModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, ItemTypeModel model) =>
        Task.FromResult(false);

    public static Task<bool> DeleteAsync(int id) =>
        Task.FromResult(false);

    public static Task<IEnumerable<ItemModel>> GetItemsAsync(int id) =>
        Task.FromResult<IEnumerable<ItemModel>>(Array.Empty<ItemModel>());

    public static bool ValidateById(long id) => false;

    public static bool ValidatedByName(string name) => false;

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                item_type_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT NOT NULL,
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