using Dapper;
using Project.DataAccess;

public class TransferServiceTests : IDisposable
{
    private readonly TransferService _service = new();
    private readonly AccessTestDatabase _database = new();

    public TransferServiceTests() => _database.CreateSchema();

    private static TransferModel ValidTransfer()
    {
        return new TransferModel
        {
            Reference = "TRANSFER-1",
            From_Location_ID = 1,
            To_Location_ID = 2,
            Transfer_Status = "pending"
        };
    }

    [Fact]
    public async Task Create_NullModel_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task Create_EmptyReference_ThrowsArgumentException()
    {
        var model = ValidTransfer();
        model.Reference = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public async Task Create_NegativeLocationId_ThrowsArgumentOutOfRangeException(
        int fromLocationId,
        int toLocationId)
    {
        var model = ValidTransfer();
        model.From_Location_ID = fromLocationId;
        model.To_Location_ID = toLocationId;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_EmptyStatus_ThrowsArgumentException()
    {
        var model = ValidTransfer();
        model.Transfer_Status = "";

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(model));
    }

    [Fact]
    public async Task Create_NonexistentFromLocation_ThrowsArgumentException()
    {
        var location = await _database.CreateLocation();
        var transfer = ValidTransfer();
        transfer.From_Location_ID = 9999;
        transfer.To_Location_ID = location.ID;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(transfer));
    }

    [Fact]
    public async Task Create_NonexistentToLocation_ThrowsArgumentException()
    {
        var location = await _database.CreateLocation();
        var transfer = ValidTransfer();
        transfer.From_Location_ID = location.ID;
        transfer.To_Location_ID = 9999;

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(transfer));
    }

    [Fact]
    public async Task Create_ValidTransfer_PersistsTransfer()
    {
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = ValidTransfer();
        transfer.From_Location_ID = from.ID;
        transfer.To_Location_ID = to.ID;

        var created = await _service.CreateAsync(transfer);
        var saved = await _service.GetByIdAsync(created.ID);

        Assert.NotNull(saved);
        Assert.Equal(created.ID, saved.ID);
        Assert.Equal("TRANSFER-1", saved.Reference);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(-1, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetItems_InvalidId_ReturnsEmpty()
    {
        var result = await _service.GetItemsAsync(-1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Commit_InvalidId_ReturnsFalse()
    {
        var result = await _service.CommitAsync(-1);

        Assert.False(result);
    }

    [Fact]
    public async Task Update_UnknownTransfer_ReturnsFalse()
    {
        var result = await _service.UpdateAsync(9999, ValidTransfer());

        Assert.False(result);
    }

    [Fact]
    public async Task Update_ProcessedTransfer_ReturnsFalse()
    {
        var transfer = await CreateTransfer("processed");

        var result = await _service.UpdateAsync(transfer.ID, ValidTransfer());

        Assert.False(result);
    }

    [Fact]
    public async Task Update_CannotSetProcessedStatus_ReturnsFalse()
    {
        var transfer = await CreateTransfer("scheduled");
        var update = ValidTransfer();
        update.From_Location_ID = transfer.From_Location_ID;
        update.To_Location_ID = transfer.To_Location_ID;
        update.Transfer_Status = "PrOcEsSeD";

        var result = await _service.UpdateAsync(transfer.ID, update);

        Assert.False(result);
    }

    [Fact]
    public async Task Update_ScheduledTransfer_UpdatesTransfer()
    {
        var transfer = await CreateTransfer("scheduled");
        var update = ValidTransfer();
        update.From_Location_ID = transfer.From_Location_ID;
        update.To_Location_ID = transfer.To_Location_ID;
        update.Reference = "UPDATED-TRANSFER";

        var result = await _service.UpdateAsync(transfer.ID, update);
        var saved = await _service.GetByIdAsync(transfer.ID);

        Assert.True(result);
        Assert.NotNull(saved);
        Assert.Equal("UPDATED-TRANSFER", saved.Reference);
    }

    [Fact]
    public async Task Commit_TransferNotScheduled_ReturnsFalse()
    {
        var transfer = await CreateTransfer("pending");

        var result = await _service.CommitAsync(transfer.ID);

        Assert.False(result);
        Assert.Equal("pending", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    [Fact]
    public async Task Commit_TransferHasNoItems_ReturnsFalseAndRollsBackStatus()
    {
        var transfer = await CreateTransfer("scheduled");

        var result = await _service.CommitAsync(transfer.ID);

        Assert.False(result);
        Assert.Equal("scheduled", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    [Fact]
    public async Task Commit_SameSourceAndDestination_ReturnsFalse()
    {
        var location = await _database.CreateLocation();
        var transfer = await CreateTransfer(
            "scheduled", location.ID, location.ID);

        var result = await _service.CommitAsync(transfer.ID);

        Assert.False(result);
        Assert.Equal("scheduled", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.5)]
    public async Task Commit_InvalidItemAmount_ReturnsFalse(decimal amount)
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await CreateTransfer("scheduled", from.ID, to.ID);
        await AddTransferItem(transfer.ID, (int)item.ID, amount);

        var result = await _service.CommitAsync(transfer.ID);

        Assert.False(result);
        Assert.Equal("scheduled", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    [Fact]
    public async Task Commit_InsufficientSourceQuantity_ReturnsFalseAndRollsBack()
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await CreateTransfer("scheduled", from.ID, to.ID);
        await AddTransferItem(transfer.ID, (int)item.ID, 5);
        var source = await InventoryAcces.Create(new InventoryModel
        {
            Item_ID = (int)item.ID,
            Location_ID = from.ID,
            Quantity_On_Hand = 2,
            Quantity_Expected = 0,
            Quantity_Ordered = 0,
            Quantity_Allocated = 0
        });

        var result = await _service.CommitAsync(transfer.ID);
        var savedSource = await InventoryAcces.GetById(source.ID);

        Assert.False(result);
        Assert.Equal(2, savedSource!.Quantity_On_Hand);
        Assert.Equal("scheduled", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    [Fact]
    public async Task Commit_MovesItemsAcrossMultipleSourceRowsAndIntoExistingDestination()
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await CreateTransfer("scheduled", from.ID, to.ID);
        await AddTransferItem(transfer.ID, (int)item.ID, 2);
        await AddTransferItem(transfer.ID, (int)item.ID, 3);
        var sourceFirst = await CreateInventory((int)item.ID, from.ID, 2);
        var sourceSecond = await CreateInventory((int)item.ID, from.ID, 5);
        var destination = await CreateInventory((int)item.ID, to.ID, 4);

        var result = await _service.CommitAsync(transfer.ID);
        var savedSourceFirst = await InventoryAcces.GetById(sourceFirst.ID);
        var savedSourceSecond = await InventoryAcces.GetById(sourceSecond.ID);
        var savedDestination = await InventoryAcces.GetById(destination.ID);
        var savedTransfer = await _service.GetByIdAsync(transfer.ID);

        Assert.True(result);
        Assert.Equal(0, savedSourceFirst!.Quantity_On_Hand);
        Assert.Equal(2, savedSourceSecond!.Quantity_On_Hand);
        Assert.Equal(9, savedDestination!.Quantity_On_Hand);
        Assert.Equal("processed", savedTransfer!.Transfer_Status);
    }

    [Fact]
    public async Task Commit_CreatesDestinationInventoryWhenMissing()
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await CreateTransfer("scheduled", from.ID, to.ID);
        await AddTransferItem(transfer.ID, (int)item.ID, 3);
        await CreateInventory((int)item.ID, from.ID, 8);

        var result = await _service.CommitAsync(transfer.ID);
        var destination = (await InventoryAcces.GetByItemId(item.ID))
            .Single(inventory => inventory.Location_ID == to.ID);

        Assert.True(result);
        Assert.Equal(3, destination.Quantity_On_Hand);
        Assert.Equal(0, destination.Quantity_Expected);
    }

    [Fact]
    public async Task Commit_DestinationOverflow_ReturnsFalseAndRollsBackSource()
    {
        var item = await _database.CreateItem();
        var from = await _database.CreateLocation();
        var to = await _database.CreateLocation();
        var transfer = await CreateTransfer("scheduled", from.ID, to.ID);
        await AddTransferItem(transfer.ID, (int)item.ID, 1);
        var source = await CreateInventory((int)item.ID, from.ID, 5);
        await CreateInventory((int)item.ID, to.ID, int.MaxValue);

        var result = await _service.CommitAsync(transfer.ID);
        var savedSource = await InventoryAcces.GetById(source.ID);

        Assert.False(result);
        Assert.Equal(5, savedSource!.Quantity_On_Hand);
        Assert.Equal("scheduled", (await _service.GetByIdAsync(transfer.ID))!.Transfer_Status);
    }

    private async Task<TransferModel> CreateTransfer(
        string status,
        int? fromId = null,
        int? toId = null)
    {
        var from = fromId.HasValue ? null : await _database.CreateLocation();
        var to = toId.HasValue ? null : await _database.CreateLocation();
        return await TransferAcces.Create(new TransferModel
        {
            Reference = "TRANSFER-TEST",
            From_Location_ID = fromId ?? from!.ID,
            To_Location_ID = toId ?? to!.ID,
            Transfer_Status = status
        });
    }

    private async Task AddTransferItem(int transferId, int itemId, decimal amount)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO Transfer_item (transfer_id, item_id, amount)
            VALUES (@transferId, @itemId, @amount);
            """,
            new { transferId, itemId, amount });
    }

    private static Task<InventoryModel> CreateInventory(
        int itemId,
        int locationId,
        int quantity) =>
        InventoryAcces.Create(new InventoryModel
        {
            Item_ID = itemId,
            Location_ID = locationId,
            Quantity_On_Hand = quantity,
            Quantity_Expected = 0,
            Quantity_Ordered = 0,
            Quantity_Allocated = 0
        });

    public void Dispose() => _database.Dispose();
}
