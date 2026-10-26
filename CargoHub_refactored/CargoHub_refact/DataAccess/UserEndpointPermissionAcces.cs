namespace Project.DataAccess;

public static class UserEndpointPermissionAcces
{
    private static readonly string _dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DataSources", "cargohub.db"));

    private const string _Table = "UserEndpointPermissions";

    public static void CreateTable(string? path = null)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL REFERENCES User(user_id),
                resource TEXT NOT NULL,
                action TEXT NOT NULL
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