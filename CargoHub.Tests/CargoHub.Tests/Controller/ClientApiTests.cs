using System.Net;
using System.Net.Http.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

public class ClientsApiTests : IDisposable
{
    private const string Url = "/api/v1/clients";

    private readonly string _dbPath;
    private readonly string? _previousDbPath;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ClientsApiTests()
    {
        _previousDbPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"cargohub-api-{Guid.NewGuid():N}.db"
        );

        // Database maken voordat de API wordt gestart.
        CreateTestDatabase();

        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _dbPath);

        UserAcces.CreateTable(_dbPath);
        UserEndpointPermissionAcces.CreateTable(_dbPath);
        var apiKey = UserAcces.Create("CargoHub client API tests")
            .GetAwaiter()
            .GetResult();
        var userId = UserAcces.GetUserId(apiKey)
            .GetAwaiter()
            .GetResult()!.Value;
        foreach (var action in new[] { "get", "post", "put", "delete" })
            UserEndpointPermissionAcces.Grant(userId, "clients", action)
                .GetAwaiter()
                .GetResult();

        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }

    private void CreateTestDatabase()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            ForeignKeys = true,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute("""
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

            CREATE TABLE "Order" (
                order_id INTEGER PRIMARY KEY AUTOINCREMENT,
                client_id INTEGER NOT NULL,
                order_date DATETIME,
                request_date DATETIME,
                reference TEXT,
                customer_po_number TEXT,
                order_status TEXT,
                shipping_notes TEXT,
                warehouse_id INTEGER,
                ship_to_client_id INTEGER,
                bill_to_client_id INTEGER,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (client_id)
                    REFERENCES Client(client_id)
            );
            """);

        // Client 1 wordt gebruikt voor de GET- en 409-tests.
        connection.Execute("""
            INSERT INTO Client (
                name, address, city, zip_code,
                province, country, contact_name,
                contact_phone, contact_email
            )
            VALUES (
                'Bestaande Client', 'Teststraat 1',
                'Rotterdam', '3011 AA',
                'Zuid-Holland', 'Netherlands',
                'Test Persoon', '0612345678',
                'test@example.com'
            );

            INSERT INTO "Order" (
                client_id, order_date, request_date,
                reference, customer_po_number,
                order_status, shipping_notes,
                warehouse_id, ship_to_client_id,
                bill_to_client_id
            )
            VALUES (
                1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
                'REF001', 'PO001', 'Pending', '',
                1, 1, 1
            );
            """);
    }

    private static ClientModel ValidClient()
    {
        return new ClientModel
        {
            Name = "Nieuwe Test Client",
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

    private async Task<ClientModel> CreateClient()
    {
        var response = await _client.PostAsJsonAsync(
            Url, ValidClient());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ClientModel>();

        return Assert.IsType<ClientModel>(created);
    }

    // TEST 1 - GET alle clients
    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clients = await response.Content
            .ReadFromJsonAsync<List<ClientModel>>();

        Assert.NotNull(clients);
        Assert.NotEmpty(clients);
    }

    // TEST 2 - GET bestaande client
    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var response = await _client.GetAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var client = await response.Content
            .ReadFromJsonAsync<ClientModel>();

        Assert.NotNull(client);
        Assert.Equal(1, client.ID);
    }

    // TEST 3 - GET onbekende client
    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await _client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // TEST 4 - GET orders van client
    [Fact]
    public async Task GetOrders_Existing_Returns200()
    {
        var response = await _client.GetAsync($"{Url}/1/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderModel>>();

        Assert.NotNull(orders);
        Assert.Single(orders);
        Assert.Equal(1, orders[0].Client_ID);
    }

    // TEST 5 - POST nieuwe client
    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var response = await _client.PostAsJsonAsync(
            Url, ValidClient());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ClientModel>();

        Assert.NotNull(created);
        Assert.True(created.ID > 1);

        // Controleren dat de client echt opgeslagen is.
        var getResponse = await _client.GetAsync(
            $"{Url}/{created.ID}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    // TEST 6 - PUT bestaande client
    [Fact]
    public async Task Update_Existing_Returns200()
    {
        var client = await CreateClient();
        client.Name = "Gewijzigde Client";

        var response = await _client.PutAsJsonAsync(
            $"{Url}/{client.ID}", client);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // TEST 7 - GET gewijzigde client
    [Fact]
    public async Task Get_UpdatedClient_ReturnsChangedData()
    {
        var client = await CreateClient();
        client.Name = "Gewijzigde Client";

        var updateResponse = await _client.PutAsJsonAsync(
            $"{Url}/{client.ID}", client);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var response = await _client.GetAsync(
            $"{Url}/{client.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content
            .ReadFromJsonAsync<ClientModel>();

        Assert.NotNull(updated);
        Assert.Equal("Gewijzigde Client", updated.Name);
    }

    // TEST 8 - DELETE testclient
    [Fact]
    public async Task Delete_Existing_Returns200()
    {
        var client = await CreateClient();

        var response = await _client.DeleteAsync(
            $"{Url}/{client.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // TEST 9 - GET verwijderde client
    [Fact]
    public async Task Get_DeletedClient_Returns404()
    {
        var client = await CreateClient();

        var deleteResponse = await _client.DeleteAsync(
            $"{Url}/{client.ID}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var response = await _client.GetAsync(
            $"{Url}/{client.ID}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // TEST 10 - POST lege naam
    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var client = ValidClient();
        client.Name = "";

        var response = await _client.PostAsJsonAsync(Url, client);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TEST 11 - POST ongeldig e-mailadres
    [Fact]
    public async Task Create_InvalidEmail_Returns400()
    {
        var client = ValidClient();
        client.Contact_Email = "ongeldig-emailadres";

        var response = await _client.PostAsJsonAsync(Url, client);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TEST 12 - POST ongeldig telefoonnummer
    [Fact]
    public async Task Create_InvalidPhone_Returns400()
    {
        var client = ValidClient();
        client.Contact_Phone = "abc123";

        var response = await _client.PostAsJsonAsync(Url, client);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TEST 13 - DELETE client met gekoppelde orders
    [Fact]
    public async Task Delete_ClientWithOrders_Returns409()
    {
        var response = await _client.DeleteAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // Client moet nog bestaan.
        var getResponse = await _client.GetAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    // TEST 14 - GET orders onbekende client
    [Fact]
    public async Task GetOrders_UnknownClient_Returns404()
    {
        var response = await _client.GetAsync(
            $"{Url}/999999/orders");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();

        Environment.SetEnvironmentVariable(
            "CARGOHUB_TEST_DB", _previousDbPath);

        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
