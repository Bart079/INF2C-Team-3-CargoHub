using Dapper;
using Project.DataAccess;

public class UserAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    [Fact]
    public void CreateTable_CreatesUserTable()
    {
        UserAcces.CreateTable(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void ResetDatabase_RecreatesUserTable()
    {
        UserAcces.CreateTable(_database.Path);

        UserAcces.ResetDatabase(_database.Path);

        Assert.True(TableExists());
    }

    [Fact]
    public void DropTable_RemovesUserTable()
    {
        UserAcces.CreateTable(_database.Path);

        UserAcces.DropTable(_database.Path);

        Assert.False(TableExists());
    }

    private bool TableExists()
    {
        using var connection = DatabaseConnection.CreateConnection(_database.Path);
        connection.Open();
        return connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'User');");
    }

    public void Dispose() => _database.Dispose();
}
