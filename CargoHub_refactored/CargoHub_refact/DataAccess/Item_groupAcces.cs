namespace Project.DataAccess;

public static class Item_groupAcces
{
    private static string _Table = "Item_group";

    // Temporary stubs for service testing; replace with database access later.
    public static Task<IEnumerable<ItemGroupModel>> GetAllAsync() =>
        Task.FromResult<IEnumerable<ItemGroupModel>>(Array.Empty<ItemGroupModel>());

    public static Task<ItemGroupModel?> GetByIdAsync(int id) =>
        Task.FromResult<ItemGroupModel?>(null);

    public static Task<ItemGroupModel> CreateAsync(ItemGroupModel model) =>
        Task.FromResult(model);

    public static Task<bool> UpdateAsync(int id, ItemGroupModel model) =>
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
                item_group_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT,
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