namespace Project.DataAccess;

public static class ClientAcces
{
    private static readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "DataSources", "cargohub.db");
    private const string _Table = "Client";

    public static void CreateTable(string? path = null)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                client_id INTEGER PRIMARY KEY AUTOINCREMENT,
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