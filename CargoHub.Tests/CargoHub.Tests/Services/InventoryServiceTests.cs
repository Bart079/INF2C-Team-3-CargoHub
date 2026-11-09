using Xunit;
using Project.DataAccess;

public class InventoryServiceTests : IDisposable
{
    private readonly InventoryService _service = new();
    private readonly AccessTestDatabase _database = new();

    public InventoryServiceTests() => _database.CreateSchema();

    private InventoryModel ValidInventory()
    {
        return new InventoryModel
        {
            Item_ID = 1,
            Location_ID = 1,
            Quantity_On_Hand = 20,
            Quantity_Expected = 5,
            Quantity_Ordered = 10,
            Quantity_Allocated = 3
        };
    }

    [Fact]  
    public async Task Create_NegativeQuantityOnHand_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inventory = ValidInventory();
        inventory.Quantity_On_Hand = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Create_NegativeQuantityExpected_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inventory = ValidInventory();
        inventory.Quantity_Expected = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Create_NegativeQuantityOrdered_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inventory = ValidInventory();
        inventory.Quantity_Ordered = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Create_NegativeQuantityAllocated_ThrowsArgumentOutOfRangeException()
    {
        //Arrange
        var inventory = ValidInventory();
        inventory.Quantity_Allocated = -1;

        //Act &Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Create_NullModel_ThrowsArgumentNullException()
    {
        //Act& Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
        () => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task Create_InvalidItemID_ThrowsArgumentException()
    {
        //Arrange
        var inventory = ValidInventory();
        inventory.Item_ID = 9999;
        //Act& Assert
        await Assert.ThrowsAsync<ArgumentException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Create_InvalidLocationID_ThrowsArgumentException()
    {
        //Arrange
        var item = await _database.CreateItem();
        await _database.CreateLocation();
        var inventory = ValidInventory();
        inventory.Item_ID = (int)item.ID;
        inventory.Location_ID = 9999;
        //Act& Assert
        await Assert.ThrowsAsync<ArgumentException>(
        () => _service.CreateAsync(inventory));
    }

    [Fact]
    public async Task Update_MismatchedId_ThrowsArgumentException()
    {
        var item = await _database.CreateItem();
        var location = await _database.CreateLocation();
        var inventory = new InventoryModel
        {
        ID = 1,
        Item_ID = (int)item.ID,
        Location_ID = location.ID,
        Quantity_On_Hand = 20,
        Quantity_Expected = 5,
        Quantity_Ordered = 10,
        Quantity_Allocated = 3
        };

        await Assert.ThrowsAsync<ArgumentException>(
        () => _service.UpdateAsync(2, inventory));
    }

    [Fact]
    public async Task Update_MatchingId_UpdatesInventory()
    {
        var item = await _database.CreateItem();
        var location = await _database.CreateLocation();
        var inventory = await InventoryAcces.Create(new InventoryModel
        {
        Item_ID = (int)item.ID,
        Location_ID = location.ID,
        Quantity_On_Hand = 20,
        Quantity_Expected = 5,
        Quantity_Ordered = 10,
        Quantity_Allocated = 3
        });
        inventory.Quantity_On_Hand = 42;

        var updated = await _service.UpdateAsync(inventory.ID, inventory);
        var saved = await InventoryAcces.GetById(inventory.ID);

        Assert.True(updated);
        Assert.NotNull(saved);
        Assert.Equal(42, saved.Quantity_On_Hand);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, ValidInventory());

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    public void Dispose() => _database.Dispose();
}