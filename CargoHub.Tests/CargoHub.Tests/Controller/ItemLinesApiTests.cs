using Microsoft.AspNetCore.Mvc;

public class ItemLineApiTests
{
    private readonly ItemLinesController _controller;

    public ItemLineApiTests()
    {
        _controller = new ItemLinesController(new FakeItemLineService());
    }

    private static ItemLineModel ValidItemLine()
    {
        return new ItemLineModel
        {
            ID = 1,
            Name = "Test",
            Description = "Test ItemType Description"
        };
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Single(
            Assert.IsAssignableFrom<IEnumerable<ItemLineModel>>(ok.Value));
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var result = await _controller.GetById(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns201()
    {
        var result = await _controller.Create(ValidItemLine());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ItemLinesController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidItemLine());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidItemLine());

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
    public async Task GetItems_ExistingItemLine_Returns200()
    {
        var result = await _controller.GetItems(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Empty(
            Assert.IsAssignableFrom<IEnumerable<ItemModel>>(ok.Value));
    }

    [Fact]
    public async Task GetItems_UnknownItemLine_Returns404()
    {
        var result = await _controller.GetItems(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    // Nepservice: geen database nodig
    private class FakeItemLineService : IItemLineService
    {
        public Task<IEnumerable<ItemLineModel>> GetAllAsync()
        {
            IEnumerable<ItemLineModel> itemLines =
                new[] { ValidItemLine() };

            return Task.FromResult(itemLines);
        }
        public Task<ItemLineModel?> GetByIdAsync(int id)
        {
            ItemLineModel? itemLine =
                id == 1 ? ValidItemLine() : null;

            return Task.FromResult(itemLine);
        }
        public Task<ItemLineModel> CreateAsync(ItemLineModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, ItemLineModel model)
        {
            return Task.FromResult(id == 1);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(id == 1);
        }

        public Task<IEnumerable<ItemModel>> GetItemsAsync(int id) =>
            Task.FromResult<IEnumerable<ItemModel>>(Array.Empty<ItemModel>());
    }
}
