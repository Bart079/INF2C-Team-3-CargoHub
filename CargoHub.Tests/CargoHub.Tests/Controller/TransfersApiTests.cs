using Microsoft.AspNetCore.Mvc;

public class TransfersApiTests
{
    private readonly TransfersController _controller =
        new(new FakeTransferService());

    private static TransferModel ValidModel() =>
        new()
        {
            ID = 1,
            Reference = "TRANSFER-1",
            From_Location_ID = 1,
            To_Location_ID = 2,
            Transfer_Status = "pending"
        };

    [Fact]
    public async Task GetAll_Returns200()
    {
        var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());
        Assert.Single(Assert.IsAssignableFrom<IEnumerable<TransferModel>>(ok.Value));
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
        Assert.Equal(nameof(TransfersController.GetById), created.ActionName);
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

    [Fact]
    public async Task GetItems_Existing_Returns200()
    {
        var ok = Assert.IsType<OkObjectResult>(await _controller.GetItems(1));
        Assert.IsAssignableFrom<IEnumerable<TransferItemModel>>(ok.Value);
    }

    [Fact]
    public async Task GetItems_Unknown_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.GetItems(2));
    }

    [Fact]
    public async Task Commit_Successful_Returns204()
    {
        Assert.IsType<NoContentResult>(await _controller.Commit(1));
    }

    [Fact]
    public async Task Commit_Unsuccessful_Returns404()
    {
        Assert.IsType<NotFoundResult>(await _controller.Commit(2));
    }

    private sealed class FakeTransferService : ITransferService
    {
        public Task<IEnumerable<TransferModel>> GetAllAsync() =>
            Task.FromResult<IEnumerable<TransferModel>>([ValidModel()]);
        public Task<TransferModel?> GetByIdAsync(int id) =>
            Task.FromResult(id == 1 ? ValidModel() : null);
        public Task<TransferModel> CreateAsync(TransferModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }
        public Task<bool> UpdateAsync(int id, TransferModel model) =>
            Task.FromResult(id == 1);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(id == 1);
        public Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id) =>
            Task.FromResult<IEnumerable<TransferItemModel>>(
                [new TransferItemModel { ID = 1, Transfer_ID = id }]);
        public Task<bool> CommitAsync(int id) => Task.FromResult(id == 1);
    }
}
