
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class WarehouseApiTests
{
    private readonly WarehouseController _controller;

    public WarehouseApiTests()
    {
        _controller = new WarehouseController(new FakeWarehouseService());
    }

    private static WarehouseModel ValidWarehouse()
    {
        return new WarehouseModel
        {
            ID = 1,
            Code = "WH001",
            Name = "Test Warehouse",
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
    public async Task GetAll_Returns200()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);

        var warehouses =
            Assert.IsAssignableFrom<IEnumerable<WarehouseModel>>(ok.Value);

        Assert.Single(warehouses);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var result = await _controller.GetById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var result = await _controller.GetById(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var result = await _controller.Create(ValidWarehouse());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(WarehouseController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(
            1, ValidWarehouse());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidWarehouse());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var result = await _controller.Delete(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetLocations_Existing_Returns200()
    {
        var result = await _controller.GetLocations(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetLocations_Unknown_Returns404()
    {
        var result = await _controller.GetLocations(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ServiceValidationError_Returns400()
    {
        var model = ValidWarehouse();
        model.Name = "INVALID";

        var result = await _controller.Create(model);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ForeignKeyConflict_Returns409()
    {
        var result = await _controller.Delete(99);

        Assert.IsType<ConflictObjectResult>(result);
    }

    private class FakeWarehouseService : IWarehouseService
    {
        public Task<IEnumerable<WarehouseModel>> GetAllAsync()
        {
            IEnumerable<WarehouseModel> warehouses =
                new[] { ValidWarehouse() };

            return Task.FromResult(warehouses);
        }

        public Task<WarehouseModel?> GetByIdAsync(int id)
        {
            WarehouseModel? warehouse =
                id == 1 ? ValidWarehouse() : null;

            return Task.FromResult(warehouse);
        }

        public Task<WarehouseModel> CreateAsync(WarehouseModel model)
        {
            if (model.Name == "INVALID")
                throw new ArgumentException("Invalid warehouse.");

            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, WarehouseModel model)
        {
            return Task.FromResult(id == 1);
        }

        public Task<bool> DeleteAsync(int id)
        {
            if (id == 99)
                throw new Microsoft.Data.Sqlite.SqliteException(
                    "FOREIGN KEY constraint failed", 19, 787);

            return Task.FromResult(id == 1);
        }

        public Task<IEnumerable<LocationModel>> GetLocationsAsync(int id)
        {
            IEnumerable<LocationModel> locations =
                Array.Empty<LocationModel>();

            return Task.FromResult(locations);
        }
    }
}
