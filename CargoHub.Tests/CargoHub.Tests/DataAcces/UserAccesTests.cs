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
    [Fact]
public async Task Create_ValidAppName_ReturnsApiKey()
{   //arrange user tabel
    UserAcces.CreateTable(_database.Path);
        //action maakt een apikey
    var apiKey = await UserAcces.Create("TestApp", _database.Path);
        //assert of apikey wordt teruggegeven & 64 tekens lang (32bytes x 2hexadecimalen)
    Assert.False(string.IsNullOrWhiteSpace(apiKey));
    Assert.Equal(64, apiKey.Length);
}

[Fact]
public async Task Create_EmptyAppName_ThrowsArgumentException()
{   //arrange tabel
    UserAcces.CreateTable(_database.Path);
    //action en assert zonder apikey
    await Assert.ThrowsAsync<ArgumentException>(
        () => UserAcces.Create("", _database.Path));
}

[Fact]
public async Task IsValid_ExistingApiKey_ReturnsTrue()
{
    UserAcces.CreateTable(_database.Path);

    var apiKey = await UserAcces.Create("TestApp", _database.Path);

    var result = await UserAcces.IsValid(apiKey, _database.Path);

    Assert.True(result);
}

    [Fact]
    public async Task IsValid_InvalidApiKey_ReturnsFalse()
    {   //arrange
        UserAcces.CreateTable(_database.Path);
        //action
        var result = await UserAcces.IsValid("WrongApiKey", _database.Path);
        //assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsValid_EmptyApiKey_ReturnsFalse()
    {   //arrange
        UserAcces.CreateTable(_database.Path);
        //action
        var result = await UserAcces.IsValid("", _database.Path);
        //assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsValid_NullApiKey_ReturnsFalse()
    {   //arrange
        UserAcces.CreateTable(_database.Path);
        //action
        var result = await UserAcces.IsValid(null!, _database.Path);
        //assert
        Assert.False(result);
    }

//extra test of de apikey hashed is in de database -> alleen gebruiker heeft de originele key
    [Fact]
    public async Task Create_StoresHashedApiKey()
    {   //arrange
        UserAcces.CreateTable(_database.Path);

        var apiKey = await UserAcces.Create("TestApp", _database.Path);
    //action verbinding en apikeyhash ophalen
        using var connection = DatabaseConnection.CreateConnection(_database.Path);
        await connection.OpenAsync();

        var storedHash = await connection.QuerySingleAsync<string>(
            "SELECT api_key_hash FROM User WHERE app_name = @AppName",
            new { AppName = "TestApp" });
    //assert
        Assert.NotEqual(apiKey, storedHash);
        Assert.Equal(64, storedHash.Length); //SHA-256 en hexstring geeft het 256/4 = 64 tekens
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
