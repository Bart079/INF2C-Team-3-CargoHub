using System.Security.Cryptography;
using System.Text;

namespace Project.DataAccess;

public static class UserAcces
{
    private const string _Table = "User";

    public static void CreateTable(string? path = null)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                api_key_hash TEXT NOT NULL UNIQUE,
                app_name TEXT NOT NULL
            );";

        using var connection = DatabaseConnection.CreateConnection(path);
        connection.Open();
        connection.Execute(sql);
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

    public static async Task<IEnumerable<UserModel>> GetAll(string? path = null)
    {
        const string sql = @"
            SELECT
                user_id AS ID,
                api_key_hash AS Api_Key_Hash,
                app_name AS App_Name
            FROM User;";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        return await connection.QueryAsync<UserModel>(sql);
    }

    public static async Task<UserModel?> GetById(int id, string? path = null)
    {
        const string sql = @"
            SELECT
                user_id AS ID,
                api_key_hash AS Api_Key_Hash,
                app_name AS App_Name
            FROM User
            WHERE user_id = @id;";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<UserModel>(sql, new { id });
    }

    public static async Task<string> Create(string appName, string? path = null)
    {
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("An app name is required.", nameof(appName));

        var apiKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await StoreApiKey(apiKey, appName, path);

        return apiKey;
    }

    public static async Task Register(string apiKey, string appName, string? path = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("An app name is required.", nameof(appName));

        await StoreApiKey(apiKey, appName, path);
    }

    private static async Task StoreApiKey(string apiKey, string appName, string? path)
    {
        var apiKeyHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));

        const string sql = @"
            INSERT INTO User (api_key_hash, app_name)
            VALUES (@ApiKeyHash, @AppName);";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, new { ApiKeyHash = apiKeyHash, AppName = appName });
    }

    public static async Task<bool> IsValid(string apiKey, string? path = null)
    {
        return await GetUserId(apiKey, path) is not null;
    }

    public static async Task<int?> GetUserId(string apiKey, string? path = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var apiKeyHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));

        const string sql = @"
            SELECT user_id
            FROM User
                WHERE api_key_hash = @ApiKeyHash
            LIMIT 1;";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<int?>(
            sql, new { ApiKeyHash = apiKeyHash });
    }
}
