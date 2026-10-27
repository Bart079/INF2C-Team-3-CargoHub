using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/item_lines")]
public class ItemLinesController : ControllerBase
{
    private readonly IItemLineService _itemLineService;

    public ItemLinesController(IItemLineService itemLineService)
    {
        _itemLineService = itemLineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemLineService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _itemLineService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemLineModel model)
    {
        var created = await _itemLineService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemLineModel model)
    {
        var updated = await _itemLineService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _itemLineService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var parent = await _itemLineService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _itemLineService.GetItemsAsync(id);
        return Ok(result);
    }
}