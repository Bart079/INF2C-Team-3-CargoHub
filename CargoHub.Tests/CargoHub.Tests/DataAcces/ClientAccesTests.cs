using Dapper;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

public class ClientAccesTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string? _previousDbPath;

    public ClientAccesTests()
    {
        _previousDbPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"cargohub-client-test-{Guid.NewGuid():N}.db"
        );

        using (File.Create(_dbPath)) { }

        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _dbPath);

        CreateTestTable();
    }

    private void CreateTestTable()
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();

        connection.Execute(@"
            CREATE TABLE Client (
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
            );
        ");
    }

    private static ClientModel ValidClient()
    {
        return new ClientModel
        {
            Name = "Test Client",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Contact_Phone = "0612345678",
            Contact_Email = "test@example.com"
        };
    }

    [Fact]
    public async Task GetAll_ReturnsClients()
    {
        await ClientAcces.Create(ValidClient());

        var clients = await ClientAcces.GetAll();

        Assert.Single(clients);
    }

    [Fact]
    public async Task GetById_ExistingClient_ReturnsClient()
    {
        var created = await ClientAcces.Create(ValidClient());

        var client = await ClientAcces.GetById(created.ID);

        Assert.NotNull(client);
        Assert.Equal(created.ID, client.ID);
        Assert.Equal("Test Client", client.Name);
    }

    [Fact]
    public async Task GetById_UnknownClient_ReturnsNull()
    {
        var client = await ClientAcces.GetById(999999);

        Assert.Null(client);
    }

    [Fact]
    public async Task Create_AddsClientToDatabase()
    {
        var model = ValidClient();

        var created = await ClientAcces.Create(model);

        Assert.True(created.ID > 0);

        var stored = await ClientAcces.GetById(created.ID);

        Assert.NotNull(stored);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Contact_Email, stored.Contact_Email);
    }

    [Fact]
    public async Task Update_ChangesClient()
    {
        var client = await ClientAcces.Create(ValidClient());

        client.Name = "Gewijzigde Client";

        var updated = await ClientAcces.Update(client.ID, client);

        Assert.True(updated);

        var stored = await ClientAcces.GetById(client.ID);

        Assert.NotNull(stored);
        Assert.Equal("Gewijzigde Client", stored.Name);
    }

    [Fact]
    public async Task Update_UnknownClient_ReturnsFalse()
    {
        var updated = await ClientAcces.Update(
            999999, ValidClient());

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesClient()
    {
        var client = await ClientAcces.Create(ValidClient());

        var deleted = await ClientAcces.Delete(client.ID);

        Assert.True(deleted);

        var stored = await ClientAcces.GetById(client.ID);

        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownClient_ReturnsFalse()
    {
        var deleted = await ClientAcces.Delete(999999);

        Assert.False(deleted);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
