using Microsoft.AspNetCore.Mvc;

public class ItemTypeApiTests
{
    private readonly ItemTypesController _controller;

    public ItemTypeApiTests()
    {
        _controller = new ItemTypesController(new FakeItemTypeService());
    }

    private static ItemTypeModel ValidItemType()
    {
        return new ItemTypeModel
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
            Assert.IsAssignableFrom<IEnumerable<ItemTypeModel>>(ok.Value));
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
        var result = await _controller.Create(ValidItemType());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ItemTypesController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidItemType());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidItemType());

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
    public async Task GetItems_ExistingItemType_Returns200()
    {
        var result = await _controller.GetItems(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Empty(
            Assert.IsAssignableFrom<IEnumerable<ItemModel>>(ok.Value));
    }

    [Fact]
    public async Task GetItems_UnknownItemType_Returns404()
    {
        var result = await _controller.GetItems(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    // Nepservice: geen database nodig
    private class FakeItemTypeService : IItemTypeService
    {
        public Task<IEnumerable<ItemTypeModel>> GetAllAsync()         
        {
            IEnumerable<ItemTypeModel> itemtype =
                new[] { ValidItemType() };

            return Task.FromResult(itemtype);
        }
        public Task<ItemTypeModel?> GetByIdAsync(int id)         
        {
            ItemTypeModel? item =
                id == 1 ? ValidItemType() : null;

            return Task.FromResult(item);
        }
        public Task<ItemTypeModel> CreateAsync(ItemTypeModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, ItemTypeModel model) 
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
