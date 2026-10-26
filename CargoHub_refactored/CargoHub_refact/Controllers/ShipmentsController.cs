using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _shipmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _shipmentService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShipmentModel model)
    {
        var created = await _shipmentService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShipmentModel model)
    {
        var updated = await _shipmentService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _shipmentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/orders")]
    public async Task<IActionResult> GetOrders(int id)
    {
        var parent = await _shipmentService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _shipmentService.GetOrdersAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var parent = await _shipmentService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _shipmentService.GetItemsAsync(id);
        return Ok(result);
    }

    [HttpPut("{id:int}/orders")]
    public async Task<IActionResult> UpdateOrders(int id, [FromBody] IEnumerable<int> orderIds)
    {
        var updated = await _shipmentService.UpdateOrdersAsync(id, orderIds);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/items")]
    public async Task<IActionResult> UpdateItems(int id, [FromBody] IEnumerable<ShipmentItemModel> items)
    {
        var updated = await _shipmentService.UpdateItemsAsync(id, items);
        if (!updated)
            return NotFound();

        return NoContent();
    }
}