namespace Project.DataAccess;

public static class UserAcces
{
    private static readonly string _dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DataSources", "cargohub.db"));

    private const string _Table = "User";

    public static void CreateTable(string? path = null)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                api_key TEXT NOT NULL,
                app_name TEXT NOT NULL
            );";

        using (var connection = CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string? path = null)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
            connection.Execute("VACUUM;");
        }
    }

    public static void ResetDatabase(string? path = null)
    {
        DropTable(path);
        CreateTable(path);
    }

    private static SqliteConnection CreateConnection(string? path)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path ?? _dbPath
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
