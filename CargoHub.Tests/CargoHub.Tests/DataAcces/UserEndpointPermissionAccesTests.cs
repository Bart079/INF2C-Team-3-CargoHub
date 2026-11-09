using Dapper;
using Project.DataAccess;

public class UserEndpointPermissionAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    [Fact]
    public void CreateTable_CreatesPermissionTable()
    {
        UserAcces.CreateTable(_database.Path);

        UserEndpointPermissionAcces.CreateTable(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void ResetDatabase_RecreatesPermissionTable()
    {
        UserAcces.CreateTable(_database.Path);
        UserEndpointPermissionAcces.CreateTable(_database.Path);

        UserEndpointPermissionAcces.ResetDatabase(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void DropTable_RemovesPermissionTable()
    {
        UserAcces.CreateTable(_database.Path);
        UserEndpointPermissionAcces.CreateTable(_database.Path);

        UserEndpointPermissionAcces.DropTable(_database.Path);

        Assert.False(TableExists());
    }

    private bool TableExists()
    {
        using var connection = DatabaseConnection.CreateConnection(_database.Path);
        connection.Open();
        return connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'UserEndpointPermissions');");
    }

    public void Dispose() => _database.Dispose();
}
