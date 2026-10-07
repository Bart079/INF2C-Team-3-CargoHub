namespace Project.DataAccess;

public static class Transfer_itemAcces
{
    private static readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "DataSources", "cargohub.db");
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=\"{_dbPath}\";");
    private static string _Table = "Transfer_item";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                transfer_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                transfer_id INTEGER NOT NULL REFERENCES Transfer(transfer_id),
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                amount decimal(10,2) NOT NULL
            );";

        using (var connection = new SqliteConnection($"Data Source=\"{path}\";"))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string path)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = new SqliteConnection($"Data Source=\"{path}\";"))
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