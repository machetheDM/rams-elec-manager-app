using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class PaymentMatchingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentMatchingService> _logger;
    private readonly QuoteService _quoteService;

    public PaymentMatchingService(AppDbContext db, ILogger<PaymentMatchingService> logger, QuoteService quoteService)
    {
        _db = db;
        _logger = logger;
        _quoteService = quoteService;
    }

    public async Task<PaymentMatch> EvaluateAsync(BankPaymentNotification notification)
    {
        // First: try to match against an approved quote payment reference
        if (!string.IsNullOrEmpty(notification.Reference))
        {
            var quote = await _db.Quotes
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q => q.PaymentReference == notification.Reference &&
                                          q.Status == QuoteStatus.Approved);

            if (quote != null)
            {
                var quotePayment = await _quoteService.RecordQuotePaymentAsync(
                    quote.Id,
                    notification.Amount ?? quote.Total,
                    notification.Reference,
                    notification.PayerName,
                    notification.Id);

                if (quotePayment != null && quote.AmountPaid + (notification.Amount ?? 0) >= quote.Total)
                {
                    var invoice = await _quoteService.ConvertToInvoiceAsync(quote.Id);
                    if (invoice != null)
                    {
                        // TODO: send receipt via SMS/WhatsApp/Email — this is a design hook
                    }
                }

                var quoteMatch = new PaymentMatch
                {
                    Id = GenerateCuid(),
                    InvoiceId = quote.ConvertedInvoiceId,
                    BankPaymentId = notification.Id,
                    Confidence = 1.0m,
                    Status = "auto_processed",
                    MatchedBy = "system",
                    ReviewReason = $"Matched quote {quote.QuoteNumber} by payment reference {quote.PaymentReference}"
                };

                _db.PaymentMatches.Add(quoteMatch);
                _db.BankPaymentNotifications.Add(notification);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Quote {Quote} paid via reference {Ref}; converted to invoice {Invoice}",
                    quote.QuoteNumber, quote.PaymentReference, quote.ConvertedInvoiceId);

                return quoteMatch;
            }
        }

        var candidates = await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue)
            .ToListAsync();

        Invoice? bestInvoice = null;
        decimal bestScore = 0;
        string? reason = null;

        foreach (var invoice in candidates)
        {
            var score = 0m;
            var reasons = new List<string>();

            if (!string.IsNullOrEmpty(notification.Reference) &&
                notification.Reference.Equals(invoice.InvoiceNumber, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.6m;
                reasons.Add("reference matches invoice number");
            }

            if (notification.Amount.HasValue && notification.Amount.Value == invoice.Total)
            {
                score += 0.3m;
                reasons.Add("amount matches total");
            }
            else if (notification.Amount.HasValue && Math.Abs(notification.Amount.Value - invoice.Total) < 1)
            {
                score += 0.2m;
                reasons.Add("amount is within rounding");
            }

            if (!string.IsNullOrEmpty(notification.PayerName) &&
                !string.IsNullOrEmpty(invoice.Customer?.Name) &&
                notification.PayerName.Contains(invoice.Customer.Name, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.1m;
                reasons.Add("payer name matches customer");
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestInvoice = invoice;
                reason = string.Join("; ", reasons);
            }
        }

        var match = new PaymentMatch
        {
            Id = GenerateCuid(),
            InvoiceId = bestInvoice?.Id,
            BankPaymentId = notification.Id,
            Confidence = bestScore,
            Status = bestScore >= 0.9m ? "auto_processed" : "pending",
            MatchedBy = bestScore >= 0.9m ? "system" : null,
            ReviewReason = reason
        };

        _db.PaymentMatches.Add(match);
        _db.BankPaymentNotifications.Add(notification);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Payment match {MatchId} confidence {Confidence} for invoice {InvoiceId}",
            match.Id, match.Confidence, match.InvoiceId);

        if (bestScore >= 0.9m && bestInvoice != null)
        {
            await RecordPaymentAsync(bestInvoice, notification.Amount ?? bestInvoice.Total, "automatic", match.Id);
        }

        return match;
    }

    public async Task<PaymentMatch?> ApproveAsync(string matchId, string approvedBy)
    {
        var match = await _db.PaymentMatches
            .Include(m => m.Invoice)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null || match.Invoice == null) return null;

        match.Status = "approved";
        match.MatchedBy = approvedBy;
        match.ReviewedAt = DateTime.UtcNow;

        await RecordPaymentAsync(match.Invoice, match.BankPayment?.Amount ?? match.Invoice.Total, approvedBy, match.Id);

        return match;
    }

    public async Task<PaymentMatch?> RejectAsync(string matchId, string reason)
    {
        var match = await _db.PaymentMatches.FindAsync(matchId);
        if (match == null) return null;

        match.Status = "rejected";
        match.ReviewReason = reason;
        match.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return match;
    }

    public async Task<List<PaymentMatch>> GetPendingMatchesAsync()
    {
        return await _db.PaymentMatches
            .Where(m => m.Status == "pending")
            .Include(m => m.Invoice)
            .Include(m => m.BankPayment)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    private async Task RecordPaymentAsync(Invoice invoice, decimal amount, string recordedBy, string matchId)
    {
        _db.Payments.Add(new Payment
        {
            Id = GenerateCuid(),
            InvoiceId = invoice.Id,
            Amount = amount,
            Method = PaymentMethod.Eft,
            Reference = matchId,
            PayerName = invoice.Customer?.Name,
            MatchedConfidence = 1.0m,
            MatchedBy = recordedBy,
            BankNotification = matchId,
            RecordedAt = DateTime.UtcNow
        });

        var totalPaid = await _db.Payments
            .Where(p => p.InvoiceId == invoice.Id)
            .SumAsync(p => p.Amount) + amount;

        invoice.PaidAt = DateTime.UtcNow;
        invoice.Status = totalPaid >= invoice.Total ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static string GenerateCuid()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        var random = Guid.NewGuid().ToString("N")[..12];
        return $"c{timestamp}{random}";
    }
}
