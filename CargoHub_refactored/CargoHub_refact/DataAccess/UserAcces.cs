namespace Project.DataAccess;

public static class UserAcces
{
    private const string _Table = "User";

    public static void CreateTable(string? path = null)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                api_key TEXT NOT NULL,
                app_name TEXT NOT NULL
            );";

        using (var connection = DatabaseConnection.CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string? path = null)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = DatabaseConnection.CreateConnection(path))
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
}
