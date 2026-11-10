public class LocationServiceTests
{
    private readonly LocationService _service = new();

    private static LocationModel ValidLocation()
    {
        return new LocationModel
        {
            ID = 1,
            Warehouse_ID = 1,
            Code = "LOC-1",
            Name = "Test Location"
        };
    }

    [Fact]
    public async Task Create_NullModel_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Theory]
    [InlineData(nameof(LocationModel.Code))]
    [InlineData(nameof(LocationModel.Name))]
    public async Task Create_EmptyRequiredText_ThrowsArgumentException(string property)
    {
        var model = ValidLocation();
        if (property == nameof(LocationModel.Code))
            model.Code = "";
        else
            model.Name = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public async Task Create_NegativeId_ThrowsArgumentOutOfRangeException(
        int locationId,
        int warehouseId)
    {
        var model = ValidLocation();
        model.ID = locationId;
        model.Warehouse_ID = warehouseId;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_ZeroId_StillValidatesRequiredFields()
    {
        var model = ValidLocation();
        model.ID = 0;
        model.Name = "";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));

        Assert.Equal(nameof(LocationModel.Name), exception.ParamName);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }
}
