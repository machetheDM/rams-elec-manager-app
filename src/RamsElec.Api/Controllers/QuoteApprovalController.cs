using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Api.Services;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("quote-approval")]
[AllowAnonymous]
public class QuoteApprovalController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly QuoteService _quoteService;

    public QuoteApprovalController(AppDbContext db, QuoteService quoteService)
    {
        _db = db;
        _quoteService = quoteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string token)
    {
        var quote = await _db.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.ApprovalToken == token);

        if (quote == null)
            return NotFound("Invalid quote link");

        if (quote.Status == QuoteStatus.Expired || quote.Status == QuoteStatus.Rejected || quote.Status == QuoteStatus.ConvertedToInvoice)
            return Content("<html><body><h1>This quote is no longer available.</h1></body></html>", "text/html");

        var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Quote {quote.QuoteNumber} from RAMS@ELEC</title>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 700px; margin: 40px auto; padding: 20px; background: #f5f5f5; }}
        .card {{ background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #1B2A4A; }}
        .line-item {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .total {{ font-size: 1.5em; font-weight: bold; margin-top: 20px; color: #1B2A4A; }}
        button {{ padding: 15px 30px; border: none; border-radius: 8px; font-size: 16px; cursor: pointer; margin-top: 20px; margin-right: 10px; }}
        .approve {{ background: #2E7D32; color: white; }}
        .reject {{ background: #C62828; color: white; }}
        textarea {{ width: 100%; padding: 10px; margin-top: 10px; border-radius: 5px; border: 1px solid #ccc; }}
    </style>
</head>
<body>
    <div class='card'>
        <h1>Quote {quote.QuoteNumber}</h1>
        <p>Hello {quote.Customer?.Name ?? "there"},</p>
        <p>Please review the quote below and select Approve or Reject.</p>

        <h3>Line Items</h3>
        {string.Join("", quote.LineItems.Select(li => $@"<div class='line-item'><span>{li.Description} x {li.Quantity}</span><span>R{li.Total:N2}</span></div>"))}

        <div class='total'>Total: R{quote.Total:N2}</div>
        <p>Expiry date: {quote.ExpiryDate:dd/MM/yyyy}</p>

        <form method='post' action='/quote-approval/{quote.Id}/approve'>
            <input type='hidden' name='token' value='{token}' />
            <button type='submit' class='approve'>Approve Quote</button>
        </form>

        <form method='post' action='/quote-approval/{quote.Id}/reject'>
            <input type='hidden' name='token' value='{token}' />
            <p>Reason for rejection (optional):</p>
            <textarea name='reason' rows='3'></textarea>
            <br />
            <button type='submit' class='reject'>Reject Quote</button>
        </form>
    </div>
</body>
</html>";

        return Content(html, "text/html");
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromForm] string token)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Id == id && q.ApprovalToken == token);
        if (quote == null) return NotFound("Invalid token");

        if (quote.Status == QuoteStatus.Expired || quote.Status == QuoteStatus.Rejected || quote.Status == QuoteStatus.ConvertedToInvoice)
            return Content("<html><body><h1>This quote is no longer available.</h1></body></html>", "text/html");

        if (quote.ExpiryDate < DateTime.UtcNow)
        {
            quote.Status = QuoteStatus.Expired;
            await _db.SaveChangesAsync();
            return Content("<html><body><h1>This quote has expired.</h1></body></html>", "text/html");
        }

        quote.Status = QuoteStatus.Approved;
        quote.PaymentReference = $"RAMS-Q-{quote.QuoteNumber}";
        quote.ApprovedAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1'>
<title>Quote Approved</title>
<style>body {{ font-family: Arial; max-width: 600px; margin: 40px auto; padding: 20px; background: #f5f5f5; }}
.card {{ background: white; padding: 30px; border-radius: 10px; }}
h1 {{ color: #2E7D32; }}</style>
</head><body>
<div class='card'>
<h1>Quote Approved</h1>
<p>Thank you! Quote <strong>{quote.QuoteNumber}</strong> has been approved.</p>
<p>Please use the following payment reference when making your EFT:</p>
<div style='background:#1B2A4A;color:#FFC107;padding:15px;border-radius:8px;font-size:1.5em;font-weight:bold;text-align:center;'>{quote.PaymentReference}</div>
<p>Bank: FNB<br/>Account: RAMS@ELEC (PTY) LTD<br/>Account No: 62816356796<br/>Branch: 210805</p>
</div>
</body></html>";

        return Content(html, "text/html");
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromForm] string token, [FromForm] string reason)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Id == id && q.ApprovalToken == token);
        if (quote == null) return NotFound("Invalid token");

        quote.Status = QuoteStatus.Rejected;
        quote.RejectionReason = reason;
        quote.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var html = "<!DOCTYPE html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1'><title>Quote Rejected</title></head><body style='font-family:Arial;max-width:600px;margin:40px auto;padding:20px'><h1>Quote Rejected</h1><p>Thank you for your feedback. Our team will contact you.</p></body></html>";
        return Content(html, "text/html");
    }
}
