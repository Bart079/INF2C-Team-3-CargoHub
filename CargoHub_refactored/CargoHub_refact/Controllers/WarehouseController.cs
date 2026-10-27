using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/warehouses")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _warehouseService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _warehouseService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WarehouseModel model)
    {
        var created = await _warehouseService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WarehouseModel model)
    {
        var updated = await _warehouseService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _warehouseService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/locations")]
    public async Task<IActionResult> GetLocations(int id)
    {
        var parent = await _warehouseService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _warehouseService.GetLocationsAsync(id);
        return Ok(result);
    }
}