using Microsoft.AspNetCore.Mvc;

public class InventoryApiTests
{
    private readonly InventoriesController _controller;

    public InventoryApiTests()
    {
        _controller = new InventoriesController(new FakeInventoryService());
    }

    private static InventoryModel ValidInventory()
    {
        return new InventoryModel
        {
            Item_ID = 1,
            Location_ID = 1,
            Quantity_On_Hand = 100,
            Quantity_Expected = 50,
            Quantity_Ordered = 25,
            Quantity_Allocated = 75
        };
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);

        var inventories =
            Assert.IsAssignableFrom<IEnumerable<InventoryModel>>(ok.Value);
        Assert.Single(inventories);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(ValidInventory().ID,
            Assert.IsType<InventoryModel>(ok.Value).ID);
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
        var result = await _controller.Create(ValidInventory());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(InventoriesController.GetById), created.ActionName);
        Assert.Equal(2, created.RouteValues["id"]);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidInventory());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(999999, ValidInventory());

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

//fake dataa
    private class FakeInventoryService : IInventoryService
    {
        public Task<IEnumerable<InventoryModel>> GetAllAsync()
        {
            IEnumerable<InventoryModel> inventories = [ValidInventory()];
            return Task.FromResult(inventories);
        }

        public Task<InventoryModel?> GetByIdAsync(int id)
        {
            InventoryModel? inventory =
                id == 1 ? ValidInventory() : null;
            return Task.FromResult(inventory);
        }

        public Task<InventoryModel> CreateAsync(InventoryModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, InventoryModel model)
        {
            return Task.FromResult(id == 1);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(id == 1);
        }
    }
}
