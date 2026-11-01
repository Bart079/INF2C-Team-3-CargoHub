using Microsoft.Data.Sqlite;

namespace Project.DataAccess;

public static class DatabaseConnection
{
    private static readonly string _dbPath = Path.Combine(
        AppContext.BaseDirectory,
        "DataSources",
        "cargohub.db"
    );

    public static SqliteConnection CreateConnection(
        string? path = null)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path ?? _dbPath,
            ForeignKeys = true
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
