namespace Project.DataAccess;

public static class TransferAcces
{
    private static readonly string _dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DataSources", "cargohub.db"));
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=\"{_dbPath}\";");
    private static string _Table = "Transfer";

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