
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class SupplierApiTests
{
    private readonly SuppliersController _controller;

    public SupplierApiTests()
    {
        _controller = new SuppliersController(new FakeSupplierService());
    }

    private static SupplierModel ValidSupplier()
    {
        return new SupplierModel
        {
            ID = 1,
            Code = "TEST001",
            Name = "Test Supplier",
            Address = "Teststraat 10",
            City = "Rotterdam",
            Zip_Code = "3011 AA",
            Province = "Zuid-Holland",
            Country = "Netherlands",
            Contact_Name = "Test Persoon",
            Phone_Number = "0612345678",
            Reference = "TEST"
        };
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Single(
            Assert.IsAssignableFrom<IEnumerable<SupplierModel>>(ok.Value));
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
        var result = await _controller.Create(ValidSupplier());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(SuppliersController.GetById),
            created.ActionName);
    }

    [Fact]
    public async Task Update_Existing_Returns204()
    {
        var result = await _controller.Update(1, ValidSupplier());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var result = await _controller.Update(
            999999, ValidSupplier());

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
    public async Task GetItems_ExistingSupplier_Returns200()
    {
        var result = await _controller.GetItems(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetItems_UnknownSupplier_Returns404()
    {
        var result = await _controller.GetItems(999999);

        Assert.IsType<NotFoundResult>(result);
    }

    // Nepservice: geen database nodig
    private class FakeSupplierService : ISupplierService
    {
        public Task<IEnumerable<SupplierModel>> GetAllAsync()
        {
            IEnumerable<SupplierModel> suppliers =
                new[] { ValidSupplier() };

            return Task.FromResult(suppliers);
        }

        public Task<SupplierModel?> GetByIdAsync(int id)
        {
            SupplierModel? supplier =
                id == 1 ? ValidSupplier() : null;

            return Task.FromResult(supplier);
        }

        public Task<SupplierModel> CreateAsync(SupplierModel model)
        {
            model.ID = 2;
            return Task.FromResult(model);
        }

        public Task<bool> UpdateAsync(int id, SupplierModel model)
        {
            return Task.FromResult(id == 1);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(id == 1);
        }

        public Task<IEnumerable<ItemModel>> GetSupplierItemsAsync(int id)
        {
            IEnumerable<ItemModel> items =
                new[] { new ItemModel { ID = 1, Supplier_ID = id } };

            return Task.FromResult(items);
        }
    }
}
