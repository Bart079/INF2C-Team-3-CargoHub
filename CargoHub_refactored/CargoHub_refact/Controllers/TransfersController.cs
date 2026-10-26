using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/transfers")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _transferService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _transferService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransferModel model)
    {
        var created = await _transferService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.ID }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TransferModel model)
    {
        var updated = await _transferService.UpdateAsync(id, model);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _transferService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id)
    {
        var parent = await _transferService.GetByIdAsync(id);
        if (parent == null)
            return NotFound();

        var result = await _transferService.GetItemsAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:int}/commit")]
    public async Task<IActionResult> Commit(int id)
    {
        var committed = await _transferService.CommitAsync(id);
        if (!committed)
            return NotFound();

        return NoContent();
    }
}