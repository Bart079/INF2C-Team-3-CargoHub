using Microsoft.AspNetCore.Mvc;

public class ItemApiTests
{
    private readonly ItemsController _controller;

    public ItemApiTests()
    {
        _controller = new ItemsController(new FakeItemsService());
    }

    private static ItemModel ValidItem()
    {
        return new ItemModel
        {
            ID = 1,
            Code = "TEST001",
            Description = "Test Item Description",
            Barcode = "1234567890123",
            Model_Number = "TEST-MODEL-001",
            Commodity_Code = 1234,
            Unit_Weight = 1.5m,
            Item_Line_ID = 1,
            Item_Group_ID = 1,
            Item_Type_ID = 1,
            Min_Purchase_Quantity = 1,
            Case_Size = 10,
            Packaging_Type = "Box",
            Order_Multiple = 1,
            Supplier_ID = 1,
            Supplier_SKU = "SUPPLIER-001",
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
            Assert.IsAssignableFrom<IEnumerable<ItemModel>>(ok.Value));
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
        var result = await _controller.Create(ValidItem());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ItemsController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidItem());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidItem());

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
    private class FakeItemsService : IItemService
    {
        public Task<IEnumerable<ItemModel>> GetAllAsync()         
        {
            IEnumerable<ItemModel> items =
                new[] { ValidItem() };

            return Task.FromResult(items);
        }
        public Task<ItemModel?> GetByIdAsync(long id)         
        {
            ItemModel? item =
                id == 1 ? ValidItem() : null;

            return Task.FromResult(item);
        }
        public Task<ItemModel> CreateAsync(ItemModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(long id, ItemModel model) 
        {
            return Task.FromResult(id == 1);
        }

        public Task<bool> DeleteAsync(long id)
        {
            return Task.FromResult(id == 1);
        }

        public Task<IEnumerable<InventoryModel>?> GetInventoryAsync(long id) =>
            Task.FromResult<IEnumerable<InventoryModel>?>(Array.Empty<InventoryModel>());

        public Task<InventoryTotalsModel?> GetInventoryTotalsAsync(long id) =>
            Task.FromResult<InventoryTotalsModel?>(null);
    }
}
