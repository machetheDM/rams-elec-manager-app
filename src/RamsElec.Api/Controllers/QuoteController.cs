using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuoteController : ControllerBase
{
    private readonly QuoteService _quoteService;
    private readonly PdfService _pdfService;
    private readonly S3Service _s3Service;

    public QuoteController(QuoteService quoteService, PdfService pdfService, S3Service s3Service)
    {
        _quoteService = quoteService;
        _pdfService = pdfService;
        _s3Service = s3Service;
    }

    [HttpGet]
    public async Task<IActionResult> GetQuotes([FromQuery] QuoteStatus? status)
    {
        var quotes = await _quoteService.GetQuotesAsync(status);
        return Ok(quotes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuote(string id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null) return NotFound();
        return Ok(quote);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteDto dto)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
        var quote = await _quoteService.CreateQuoteAsync(dto, email);
        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendQuote(string id)
    {
        var quote = await _quoteService.SendQuoteAsync(id);
        if (quote == null) return NotFound();
        return Ok(new { quote.Id, quote.QuoteNumber, quote.ApprovalToken });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveQuote(string id)
    {
        var quote = await _quoteService.ApproveQuoteAsync(id);
        if (quote == null) return NotFound();
        return Ok(new { quote.Id, quote.QuoteNumber, quote.PaymentReference, status = quote.Status });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectQuote(string id, [FromBody] RejectQuoteDto dto)
    {
        var quote = await _quoteService.RejectQuoteAsync(id, dto.Reason);
        if (quote == null) return NotFound();
        return Ok(quote);
    }

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> ConvertToInvoice(string id)
    {
        var invoice = await _quoteService.ConvertToInvoiceAsync(id);
        if (invoice == null) return BadRequest(new { message = "Quote not found or not fully paid" });
        return Ok(invoice);
    }
}

public class RejectQuoteDto
{
    public string Reason { get; set; } = string.Empty;
}
