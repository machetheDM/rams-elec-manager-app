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
    private readonly S3Service _s3Service;
    private readonly TwilioService _twilioService;
    private readonly WhatsAppService _whatsAppService;

    public InvoiceController(InvoiceService invoiceService, PdfService pdfService,
        S3Service s3Service, TwilioService twilioService, WhatsAppService whatsAppService)
    {
        _invoiceService = invoiceService;
        _pdfService = pdfService;
        _s3Service = s3Service;
        _twilioService = twilioService;
        _whatsAppService = whatsAppService;
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
        var invoice = await _invoiceService.GetInvoice(id);
        if (invoice == null) return NotFound();

        var pdf = _pdfService.GenerateInvoicePdf(invoice);
        var s3Key = await _s3Service.UploadInvoicePdfAsync(invoice.InvoiceNumber, pdf);
        var presignedUrl = await _s3Service.GetPresignedUrlAsync(s3Key);

        var phone = dto.RecipientPhone ?? invoice.Customer?.Phone;
        string? result = null;

        if (dto.Channel == DeliveryChannel.Sms && !string.IsNullOrEmpty(phone))
        {
            result = await _twilioService.SendInvoiceLinkAsync(phone, presignedUrl, invoice.InvoiceNumber);
        }
        else if (dto.Channel == DeliveryChannel.WhatsApp && !string.IsNullOrEmpty(phone))
        {
            result = await _whatsAppService.SendInvoiceLinkAsync(phone, presignedUrl, invoice.InvoiceNumber, invoice.Customer?.Name);
        }

        var updated = await _invoiceService.MarkAsSent(id, dto.Channel, presignedUrl);

        return Ok(new
        {
            message = $"Invoice {invoice.InvoiceNumber} sent via {dto.Channel}",
            pdfUrl = presignedUrl,
            deliveryResult = result
        });
    }

    [HttpPost("{id}/pay")]
    public IActionResult RecordPayment(string id, [FromBody] RecordPaymentDto dto)
    {
        // This endpoint is now handled by PaymentController; kept for backward compatibility
        return Ok(new { message = "Use POST /api/payment" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(string id, [FromBody] CreateInvoiceDto dto)
    {
        var invoice = await _invoiceService.UpdateInvoice(id, dto);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(string id)
    {
        var deleted = await _invoiceService.DeleteInvoice(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelInvoice(string id)
    {
        var invoice = await _invoiceService.CancelInvoice(id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }
}
