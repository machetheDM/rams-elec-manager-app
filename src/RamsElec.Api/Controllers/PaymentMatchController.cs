using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentMatchController : ControllerBase
{
    private readonly PaymentMatchingService _paymentMatchingService;

    public PaymentMatchController(PaymentMatchingService paymentMatchingService)
    {
        _paymentMatchingService = paymentMatchingService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var matches = await _paymentMatchingService.GetPendingMatchesAsync();
        return Ok(matches);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id)
    {
        var user = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "manager";
        var match = await _paymentMatchingService.ApproveAsync(id, user);
        if (match == null) return NotFound();
        return Ok(match);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromBody] RejectMatchDto dto)
    {
        var match = await _paymentMatchingService.RejectAsync(id, dto.Reason);
        if (match == null) return NotFound();
        return Ok(match);
    }
}

public class RejectMatchDto
{
    public string Reason { get; set; } = string.Empty;
}
