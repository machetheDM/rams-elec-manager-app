using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly PdfService _pdfService;

    public InvoiceController(InvoiceService invoiceService, PdfService pdfService)
    {
        _invoiceService = invoiceService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceStatus? status)
    {
        var invoices = await _invoiceService.GetInvoices(status);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoice(string id)
    {
        var invoice = await _invoiceService.GetInvoice(id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
        var invoice = await _invoiceService.CreateInvoice(dto, email);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(string id)
    {
        var invoice = await _invoiceService.GetInvoice(id);
        if (invoice == null) return NotFound();

        var pdf = _pdfService.GenerateInvoicePdf(invoice);
        return File(pdf, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendInvoice(string id, [FromBody] SendInvoiceDto dto)
    {
        var invoice = await _invoiceService.MarkAsSent(id, dto.Channel);
        if (invoice == null) return NotFound();

        // TODO: Phase 3 — trigger Twilio SMS or n8n WhatsApp webhook
        return Ok(new { message = $"Invoice {invoice.InvoiceNumber} marked as sent via {dto.Channel}" });
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> RecordPayment(string id, [FromBody] RecordPaymentDto dto)
    {
        var invoice = await _invoiceService.MarkAsPaid(id);
        if (invoice == null) return NotFound();

        // TODO: Phase 3 — persist payment record, generate receipt
        return Ok(new { message = $"Invoice {invoice.InvoiceNumber} marked as paid" });
    }
}
