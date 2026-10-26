using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/item_types")]
public class ItemTypesController : ControllerBase
{
    private readonly IItemTypeService _itemTypeService;

    public ItemTypesController(IItemTypeService itemTypeService)
    {
        _itemTypeService = itemTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemTypeService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _itemTypeService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemTypeModel model)
    {
        var created = await _itemTypeService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemTypeModel model)
    {
        var updated = await _itemTypeService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _itemTypeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var parent = await _itemTypeService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _itemTypeService.GetItemsAsync(id);
        return Ok(result);
    }
}