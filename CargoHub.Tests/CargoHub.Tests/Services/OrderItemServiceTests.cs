public class OrderItemServiceTests
{
    private readonly OrderItemService _service = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetItems_NonPositiveId_ReturnsEmpty(int id)
    {
        var result = await _service.GetItemsAsync(id);

        Assert.Empty(result);
    }
}
