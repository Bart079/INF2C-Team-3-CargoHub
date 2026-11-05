using Project.DataAccess;

namespace CargoHub.Tests.DataAcces;

[Collection("ShipmentAcces")]
public class ShipmentAccesTests
{
    [Fact]
    public async Task Create_SavesShipmentToDatabase()
    {
        using var database = new TestDatabase();
        database.Initialize();

        var created = await ShipmentAcces.Create(CreateShipment());
        var saved = await ShipmentAcces.GetById(created.ID);

        Assert.NotNull(saved);
        Assert.Equal(created.ID, saved.ID);
        Assert.Equal("test-shipment", saved.Reference);
    }

    [Fact]
    public async Task GetAll_ReturnsSavedShipments()
    {
        using var database = new TestDatabase();
        database.Initialize();
        await ShipmentAcces.Create(CreateShipment("first"));
        await ShipmentAcces.Create(CreateShipment("second"));

        var shipments = (await ShipmentAcces.GetAll()).ToList();

        Assert.Equal(2, shipments.Count);
        Assert.Contains(shipments, shipment => shipment.Reference == "first");
        Assert.Contains(shipments, shipment => shipment.Reference == "second");
    }

    [Fact]
    public async Task GetById_ReturnsSavedShipment()
    {
        using var database = new TestDatabase();
        database.Initialize();
        var created = await ShipmentAcces.Create(CreateShipment());

        var saved = await ShipmentAcces.GetById(created.ID);

        Assert.NotNull(saved);
        Assert.Equal(created.Reference, saved.Reference);
        Assert.Equal(created.Order_ID, saved.Order_ID);
    }

    [Fact]
    public async Task Update_SavesShipmentChanges()
    {
        using var database = new TestDatabase();
        database.Initialize();
        var created = await ShipmentAcces.Create(CreateShipment());
        var updatedShipment = CreateShipment("updated-shipment", orderId: 2);

        await ShipmentAcces.Update(created.ID, updatedShipment);
        var saved = await ShipmentAcces.GetById(created.ID);

        Assert.NotNull(saved);
        Assert.Equal(updatedShipment.Reference, saved.Reference);
        Assert.Equal(updatedShipment.Order_ID, saved.Order_ID);
    }

    [Fact]
    public async Task Delete_RemovesShipmentFromDatabase()
    {
        using var database = new TestDatabase();
        database.Initialize();
        var created = await ShipmentAcces.Create(CreateShipment());

        await ShipmentAcces.Delete(created.ID);
        var saved = await ShipmentAcces.GetById(created.ID);

        Assert.Null(saved);
    }

    private static ShipmentModel CreateShipment(
        string reference = "test-shipment",
        int orderId = 1)
    {
        return new ShipmentModel
        {
            Order_ID = orderId,
            Reference = reference,
            Shipment_Date = new DateTime(2026, 1, 2, 3, 4, 5),
            Shipment_Type = "Parcel",
            Shipment_Status = "Pending",
            Carrier_Name = "Test Carrier",
            Shipping_Method = "Road",
            Payment_Type = "Prepaid"
        };
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string? _previousPath =
            Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");

        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"shipment-tests-{Guid.NewGuid():N}.db");

        public TestDatabase()
        {
            using (File.Create(Path))
            {
            }

            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", Path);
        }

        public void Initialize()
        {
            using (var connection = DatabaseConnection.CreateConnection(Path))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE "Order" (order_id INTEGER PRIMARY KEY);
                    INSERT INTO "Order" (order_id) VALUES (1), (2);
                    """;
                command.ExecuteNonQuery();
            }

            ShipmentAcces.CreateTable(Path);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", _previousPath);
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}

[CollectionDefinition("ShipmentAcces", DisableParallelization = true)]
public sealed class ShipmentAccesCollection
{
}
