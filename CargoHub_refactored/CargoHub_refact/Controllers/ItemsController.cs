using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _itemService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemModel model)
    {
        var created = await _itemService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ItemModel model)
    {
        var updated = await _itemService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _itemService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:long}/inventory")]
    public async Task<IActionResult> GetInventory(long id)
    {
        var parent = await _itemService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _itemService.GetInventoryAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:long}/inventory/totals")]
    public async Task<IActionResult> GetInventoryTotals(long id)
    {
        var parent = await _itemService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _itemService.GetInventoryTotalsAsync(id);
        return Ok(result);
    }
}