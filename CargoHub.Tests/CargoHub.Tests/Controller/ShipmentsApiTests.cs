using System.Net;
using System.Net.Http.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

public class ShipmentsApiTests : IDisposable
{
    private const string Url = "/api/v1/shipments";

    private readonly string _dbPath;
    private readonly string? _previousDbPath;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ShipmentsApiTests()
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

        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
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
            CREATE TABLE "Order" (
                order_id INTEGER PRIMARY KEY AUTOINCREMENT
            );

            CREATE TABLE Item (
                item_id INTEGER PRIMARY KEY AUTOINCREMENT
            );

            CREATE TABLE Shipment (
                shipment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_id INTEGER NOT NULL REFERENCES "Order"(order_id),
                reference TEXT NOT NULL,
                shipment_date DATETIME NOT NULL,
                shipment_type TEXT NOT NULL,
                shipment_status TEXT NOT NULL,
                carrier_name TEXT NOT NULL,
                shipping_method TEXT NOT NULL,
                payment_type TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE Shipment_item (
                shipment_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                shipment_id INTEGER NOT NULL
                    REFERENCES Shipment(shipment_id),
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                amount INTEGER NOT NULL
            );

            CREATE TABLE Inventory (
                inventory_id INTEGER PRIMARY KEY AUTOINCREMENT,
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                location_id INTEGER NOT NULL,
                quantity_on_hand INTEGER NOT NULL,
                quantity_expected INTEGER NOT NULL,
                quantity_ordered INTEGER NOT NULL,
                quantity_allocated INTEGER NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
            """);

        // Shipment 1 wordt gebruikt voor de GET- en 409-tests.
        connection.Execute("""
            INSERT INTO "Order" (order_id) VALUES (1), (2);
            INSERT INTO Item (item_id) VALUES (1);

            INSERT INTO Shipment (
                shipment_id, order_id, reference, shipment_date,
                shipment_type, shipment_status, carrier_name,
                shipping_method, payment_type
            )
            VALUES (
                1, 1, 'SHIP001', '2026-01-15',
                'Standard', 'Scheduled', 'Test Carrier',
                'Standard', 'Manual'
            );

            INSERT INTO Shipment_item (
                shipment_item_id, shipment_id, item_id, amount
            )
            VALUES (1, 1, 1, 2);

            INSERT INTO Inventory (
                item_id, location_id, quantity_on_hand,
                quantity_expected, quantity_ordered,
                quantity_allocated
            )
            VALUES (1, 1, 10, 0, 2, 0);
            """);
    }

    private static ShipmentModel ValidShipment()
    {
        return new ShipmentModel
        {
            Order_ID = 1,
            Reference = "SHIP002",
            Shipment_Date = new DateTime(2026, 1, 15),
            Shipment_Type = "Standard",
            Shipment_Status = "Scheduled",
            Carrier_Name = "Test Carrier",
            Shipping_Method = "Standard",
            Payment_Type = "Manual"
        };
    }

    private async Task<ShipmentModel> CreateShipment()
    {
        var response = await _client.PostAsJsonAsync(
            Url, ValidShipment());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ShipmentModel>();

        return Assert.IsType<ShipmentModel>(created);
    }

    // TEST 1 - GET alle shipments
    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _client.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var shipments = await response.Content
            .ReadFromJsonAsync<List<ShipmentModel>>();

        Assert.NotNull(shipments);
        Assert.NotEmpty(shipments);
    }

    // TEST 2 - GET bestaande shipment
    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var response = await _client.GetAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var shipment = await response.Content
            .ReadFromJsonAsync<ShipmentModel>();

        Assert.NotNull(shipment);
        Assert.Equal(1, shipment.ID);
    }

    // TEST 3 - GET onbekende shipment
    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var response = await _client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // TEST 4 - GET orders van shipment
    [Fact]
    public async Task GetOrders_Existing_Returns200()
    {
        var response = await _client.GetAsync($"{Url}/1/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<int>>();

        Assert.NotNull(orders);
        Assert.Single(orders);
        Assert.Equal(1, orders[0]);
    }

    // TEST 5 - GET items van shipment
    [Fact]
    public async Task GetItems_Existing_Returns200()
    {
        var response = await _client.GetAsync($"{Url}/1/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content
            .ReadFromJsonAsync<List<ShipmentItemModel>>();

        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Equal(1, items[0].Item_ID);
        Assert.Equal(2, items[0].Amount);
    }

    // TEST 6 - POST nieuwe shipment
    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var response = await _client.PostAsJsonAsync(
            Url, ValidShipment());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ShipmentModel>();

        Assert.NotNull(created);
        Assert.True(created.ID > 1);

        // Controleren dat de shipment echt opgeslagen is.
        var getResponse = await _client.GetAsync(
            $"{Url}/{created.ID}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    // TEST 7 - PUT bestaande shipment
    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var shipment = await CreateShipment();
        shipment.Reference = "SHIP-UPDATED";

        var response = await _client.PutAsJsonAsync(
            $"{Url}/{shipment.ID}", shipment);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // TEST 8 - GET gewijzigde shipment
    [Fact]
    public async Task Get_UpdatedShipment_ReturnsChangedData()
    {
        var shipment = await CreateShipment();
        shipment.Reference = "SHIP-UPDATED";

        var updateResponse = await _client.PutAsJsonAsync(
            $"{Url}/{shipment.ID}", shipment);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var response = await _client.GetAsync(
            $"{Url}/{shipment.ID}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content
            .ReadFromJsonAsync<ShipmentModel>();

        Assert.NotNull(updated);
        Assert.Equal("SHIP-UPDATED", updated.Reference);
    }

    // TEST 9 - PUT order van shipment
    [Fact]
    public async Task UpdateOrders_Existing_Returns204()
    {
        var response = await _client.PutAsJsonAsync(
            $"{Url}/1/orders", new[] { 2 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"{Url}/1/orders");
        var orders = await getResponse.Content
            .ReadFromJsonAsync<List<int>>();

        Assert.NotNull(orders);
        Assert.Equal(new[] { 2 }, orders);
    }

    // TEST 10 - PUT items van shipment
    [Fact]
    public async Task UpdateItems_Existing_Returns204()
    {
        var items = new[]
        {
            new ShipmentItemModel(0, 1, 1, 4)
        };

        var response = await _client.PutAsJsonAsync(
            $"{Url}/1/items", items);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"{Url}/1/items");
        var updatedItems = await getResponse.Content
            .ReadFromJsonAsync<List<ShipmentItemModel>>();

        Assert.NotNull(updatedItems);
        Assert.Single(updatedItems);
        Assert.Equal(4, updatedItems[0].Amount);
    }

    // TEST 11 - DELETE shipment zonder gekoppelde items
    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var shipment = await CreateShipment();

        var response = await _client.DeleteAsync(
            $"{Url}/{shipment.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // TEST 12 - DELETE shipment met gekoppelde items
    [Fact]
    public async Task Delete_ShipmentWithItems_Returns409()
    {
        var response = await _client.DeleteAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // Shipment moet nog bestaan.
        var getResponse = await _client.GetAsync($"{Url}/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    // TEST 13 - GET orders van onbekende shipment
    [Fact]
    public async Task GetOrders_UnknownShipment_Returns404()
    {
        var response = await _client.GetAsync(
            $"{Url}/999999/orders");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // TEST 14 - GET items van onbekende shipment
    [Fact]
    public async Task GetItems_UnknownShipment_Returns404()
    {
        var response = await _client.GetAsync(
            $"{Url}/999999/items");

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
