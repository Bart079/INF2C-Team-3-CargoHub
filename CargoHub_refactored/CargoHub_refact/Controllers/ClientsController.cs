
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/clients")]
public class ClientsController : ControllerBase
{
    // Service die de controller gebruikt
    private readonly IClientService _clientService;

    // Constructor: service wordt meegegeven
    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("ClientsController work!!!");
    }

    // GET: api/v1/clients
    // Alle clients ophalen
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    // GET: api/v1/clients/1
    // Een client ophalen op basis van ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);

        if (client == null)
            return NotFound();

        return Ok(client);
    }

    // GET: api/v1/clients/1/orders
    // Alle orders van een client ophalen
    [HttpGet("{id:int}/orders")]
    public async Task<IActionResult> GetOrders(int id)
    {
        var orders = await _clientService.GetOrdersAsync(id);
        return Ok(orders);
    }

    // POST: api/v1/clients
    // Nieuwe client toevoegen
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ClientModel client)
    {
        var createdClient = await _clientService.CreateAsync(client);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdClient.ID },
            createdClient
        );
    }

    // PUT: api/v1/clients/1
    // Bestaande client aanpassen
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ClientModel client)
    {
        var updated = await _clientService.UpdateAsync(id, client);

        if (!updated)
            return NotFound();

        return Ok();
    }

    // DELETE: api/v1/clients/1
    // Client verwijderen
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _clientService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return Ok();
    }
}
