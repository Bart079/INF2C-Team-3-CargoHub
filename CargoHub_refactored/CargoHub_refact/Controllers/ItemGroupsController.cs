using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/item_groups")]
public class ItemGroupsController : ControllerBase
{
    private readonly IItemGroupService _itemGroupService;

    public ItemGroupsController(IItemGroupService itemGroupService)
    {
        _itemGroupService = itemGroupService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemGroupService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _itemGroupService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemGroupModel model)
    {
        var created = await _itemGroupService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemGroupModel model)
    {
        var updated = await _itemGroupService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _itemGroupService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var parent = await _itemGroupService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _itemGroupService.GetItemsAsync(id);
        return Ok(result);
    }
}