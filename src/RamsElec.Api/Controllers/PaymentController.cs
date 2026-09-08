using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto dto)
    {
        var payment = await _paymentService.RecordPaymentAsync(dto);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    [HttpPost("fnb")]
    public async Task<IActionResult> RecordFnbPayment([FromBody] FnbPaymentDto dto)
    {
        var recordPayment = new RecordPaymentDto
        {
            InvoiceId = dto.InvoiceId,
            Amount = dto.Amount,
            Method = PaymentMethod.FnbSpeedPoint,
            Reference = $"FNB-{dto.ApprovalCode}-{dto.LastFourDigits}",
            PayerName = dto.PayerName
        };

        var payment = await _paymentService.RecordPaymentAsync(recordPayment);
        if (payment == null) return NotFound(new { message = "Invoice not found" });
        return Ok(payment);
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<IActionResult> GetPayments(string invoiceId)
    {
        var payments = await _paymentService.GetPaymentsForInvoiceAsync(invoiceId);
        return Ok(payments);
    }
}
