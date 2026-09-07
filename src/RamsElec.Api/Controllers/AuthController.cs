using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(AuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        // In production, validate against the shared database users table
        // For now, validate against configured admin credentials
        var adminEmail = _config["Admin:Email"] ?? "admin@ramsatelec.com";
        var adminPassword = _config["Admin:Password"] ?? "password123";

        if (dto.Email != adminEmail || dto.Password != adminPassword)
            return Unauthorized(new { message = "Invalid credentials" });

        var response = _authService.GenerateToken(dto.Email, "Admin", "admin");
        return Ok(response);
    }
}
