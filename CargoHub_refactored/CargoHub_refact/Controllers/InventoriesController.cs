using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/inventories")]
public class InventoriesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoriesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _inventoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _inventoryService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryModel model)
    {
        var created = await _inventoryService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InventoryModel model)
    {
        var updated = await _inventoryService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _inventoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
