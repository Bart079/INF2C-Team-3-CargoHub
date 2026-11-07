using Xunit;

public class InventoryServiceTests
{
    private readonly InventoryService _service = new();

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
        var inventory = ValidInventory();
        inventory.Location_ID = 9999;
        //Act& Assert
        await Assert.ThrowsAsync<ArgumentException>(
        () => _service.CreateAsync(inventory));
    }
}