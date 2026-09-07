using Microsoft.AspNetCore.Mvc;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        service = "RamsElec.Api",
        timestamp = DateTime.UtcNow
    });
}
