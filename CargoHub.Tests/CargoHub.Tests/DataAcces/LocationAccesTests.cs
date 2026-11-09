using Project.DataAccess;

public class LocationAccesTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    public LocationAccesTests() => _database.CreateSchema();

    [Fact]
    public async Task GetAll_ReturnsLocations()
    {
        await _database.CreateLocation();

        var locations = await LocationAcces.GetAll();

        Assert.Single(locations);
    }

    [Fact]
    public async Task GetById_ExistingLocation_ReturnsLocation()
    {
        var created = await _database.CreateLocation();

        var found = await LocationAcces.GetById(created.ID);

        Assert.NotNull(found);
        Assert.Equal(created.ID, found.ID);
        Assert.Equal(created.Code, found.Code);
    }

    [Fact]
    public async Task GetById_UnknownLocation_ReturnsNull()
    {
        var location = await LocationAcces.GetById(999999);

        Assert.Null(location);
    }

    [Fact]
    public async Task Create_AddsLocationToDatabase()
    {
        var existingLocation = await _database.CreateLocation();
        var model = new LocationModel
        {
            Warehouse_ID = existingLocation.Warehouse_ID,
            Code = $"LOC-{Guid.NewGuid():N}",
            Name = "Created Location"
        };

        var created = await LocationAcces.Create(model);
        var stored = await LocationAcces.GetById(created.ID);

        Assert.True(created.ID > 0);
        Assert.NotNull(stored);
        Assert.Equal(model.Name, stored.Name);
        Assert.Equal(model.Code, stored.Code);
    }

    [Fact]
    public async Task Update_ChangesLocation()
    {
        var location = await _database.CreateLocation();
        location.Name = "Updated Location";

        var updated = await LocationAcces.Update(location.ID, location);
        var stored = await LocationAcces.GetById(location.ID);

        Assert.True(updated);
        Assert.NotNull(stored);
        Assert.Equal("Updated Location", stored.Name);
    }

    [Fact]
    public async Task Update_UnknownLocation_ReturnsFalse()
    {
        var existingLocation = await _database.CreateLocation();
        var model = new LocationModel
        {
            Warehouse_ID = existingLocation.Warehouse_ID,
            Code = "LOC-UNKNOWN",
            Name = "Unknown"
        };

        var updated = await LocationAcces.Update(999999, model);

        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_RemovesLocation()
    {
        var location = await _database.CreateLocation();

        var deleted = await LocationAcces.Delete(location.ID);
        var stored = await LocationAcces.GetById(location.ID);

        Assert.True(deleted);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_UnknownLocation_ReturnsFalse()
    {
        var deleted = await LocationAcces.Delete(999999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ValidatedById_ExistingLocation_ReturnsFalse()
    {
        var location = await _database.CreateLocation();

        Assert.False(LocationAcces.ValidatedById(location.ID));
    }

    [Fact]
    public void ValidatedById_UnknownLocation_ReturnsTrue()
    {
        Assert.True(LocationAcces.ValidatedById(int.MaxValue));
    }

    [Fact]
    public async Task GetWarehousesLocations_ReturnsOnlyLocationsInWarehouse()
    {
        var first = await _database.CreateLocation();
        var second = await LocationAcces.Create(new LocationModel
        {
            Warehouse_ID = first.Warehouse_ID,
            Code = $"LOC-{Guid.NewGuid():N}",
            Name = "Another Location"
        });
        await _database.CreateLocation();

        var locations = await LocationAcces.GetWarehousesLocations(first.Warehouse_ID);

        Assert.Equal(2, locations.Count());
        Assert.Contains(locations, location => location.ID == first.ID);
        Assert.Contains(locations, location => location.ID == second.ID);
    }

    public void Dispose() => _database.Dispose();
}
