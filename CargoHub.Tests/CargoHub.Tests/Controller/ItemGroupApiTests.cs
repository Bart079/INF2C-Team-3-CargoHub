using Microsoft.AspNetCore.Mvc;

public class ItemGroupApiTests
{
    private readonly ItemGroupsController _controller;

    public ItemGroupApiTests()
    {
        _controller = new ItemGroupsController(new FakeItemGroupsService());
    }

    private static ItemGroupModel ValidItemGroup()
    {
        return new ItemGroupModel
        {
            ID = 1,
            Name = "TEST001",
            Description = "Test ItemGroup Description",
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Single(
            Assert.IsAssignableFrom<IEnumerable<ItemGroupModel>>(ok.Value));
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
        var result = await _controller.Create(ValidItemGroup());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ItemGroupsController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidItemGroup());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidItemGroup());

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


    // Nepservice: geen database nodig
    private class FakeItemGroupsService : IItemGroupService
    {
        public Task<IEnumerable<ItemGroupModel>> GetAllAsync()         
        {
            IEnumerable<ItemGroupModel> items =
                new[] { ValidItemGroup() };

            return Task.FromResult(items);
        }
        public Task<ItemGroupModel?> GetByIdAsync(int id)
        {
            ItemGroupModel? item =
                id == 1 ? ValidItemGroup() : null;

            return Task.FromResult(item);
        }
        public Task<ItemGroupModel> CreateAsync(ItemGroupModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, ItemGroupModel model)
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
