namespace Project.DataAccess;

using Dapper;

public static class UserEndpointPermissionAcces
{
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

    public static async Task Grant(
        int userId,
        string resource,
        string action,
        string? path = null)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("A resource is required.", nameof(resource));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("An action is required.", nameof(action));

        const string sql = $@"
            INSERT INTO {_Table} (user_id, resource, action)
            VALUES (@UserId, @Resource, @Action);";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, new { UserId = userId, Resource = resource, Action = action });
    }

    public static async Task<bool> HasPermission(
        int userId,
        string resource,
        string action,
        string? path = null)
    {
        const string sql = $@"
            SELECT EXISTS(
                SELECT 1
                FROM {_Table}
                WHERE user_id = @UserId
                    AND resource = @Resource
                    AND action = @Action
            );";

        using var connection = DatabaseConnection.CreateConnection(path);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<bool>(
            sql, new { UserId = userId, Resource = resource, Action = action });
    }
}