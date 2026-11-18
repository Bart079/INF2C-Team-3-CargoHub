using Microsoft.Data.Sqlite;

namespace Project.DataAccess;

public static class DatabaseConnection
{
    private static readonly string _dbPath = Path.Combine(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")),
        "DataSources",
        "cargohub.db"
    );

    public static SqliteConnection CreateConnection(string? path = null)
    {
        var testPath = Environment.GetEnvironmentVariable(
            "CARGOHUB_TEST_DB");
        
        var configurePath = Environment.GetEnvironmentVariable(
            "CARGOHUB_DB_PATH");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path ?? testPath ?? configurePath ?? _dbPath,
            Mode = SqliteOpenMode.ReadWrite,
            ForeignKeys = true,
            Pooling = testPath == null
        }.ToString();

        return new SqliteConnection(connectionString);
    }

    
}