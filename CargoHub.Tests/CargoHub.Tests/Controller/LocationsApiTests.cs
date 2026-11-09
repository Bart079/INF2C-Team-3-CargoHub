using Microsoft.AspNetCore.Mvc;

public class LocationsApiTests
{
    private readonly LocationsController _controller =
        new(new FakeLocationService());

    private static LocationModel ValidModel() =>
        new() { ID = 1, Warehouse_ID = 1, Code = "LOC-1", Name = "Location" };

    [Fact]
    public async Task GetAll_Returns200()
    {
        var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());
        Assert.Single(Assert.IsAssignableFrom<IEnumerable<LocationModel>>(ok.Value));
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        Assert.IsType<OkObjectResult>(await _controller.GetById(1));
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.GetById(2));
    }

    [Fact]
    public async Task Create_Returns201WithRouteId()
    {
        var created = Assert.IsType<CreatedAtActionResult>(
            await _controller.Create(ValidModel()));
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(LocationsController.GetById), created.ActionName);
        Assert.Equal(2, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        Assert.IsType<NoContentResult>(await _controller.Update(1, ValidModel()));
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.Update(2, ValidModel()));
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        Assert.IsType<NoContentResult>(await _controller.Delete(1));
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.Delete(2));
    }

    private sealed class FakeLocationService : ILocationService
    {
        public Task<IEnumerable<LocationModel>> GetAllAsync() =>
            Task.FromResult<IEnumerable<LocationModel>>([ValidModel()]);
        public Task<LocationModel?> GetByIdAsync(int id) =>
            Task.FromResult(id == 1 ? ValidModel() : null);
        public Task<LocationModel> CreateAsync(LocationModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }
        public Task<bool> UpdateAsync(int id, LocationModel model) =>
            Task.FromResult(id == 1);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(id == 1);
    }
}
