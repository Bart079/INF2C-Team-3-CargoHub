using Microsoft.AspNetCore.Mvc;
using Project.Middleware;

[ApiController]
[Route("api/test/exception")]
[RequirePermission("clients")]
public class ExceptionTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        throw new InvalidOperationException(
            "SECRET_INTERNAL_ERROR_123");
    }
}