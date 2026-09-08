using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class QuoteService
{
    private readonly AppDbContext _db;

    public QuoteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Quote> CreateQuoteAsync(CreateQuoteDto dto, string createdBy)
    {
        var quoteNumber = await GenerateQuoteNumber();

        var lineItems = dto.LineItems.Select(li => new QuoteLineItem
        {
            Id = GenerateCuid(),
            Description = li.Description,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            Total = li.Quantity * li.UnitPrice,
            Category = li.Category,
            SortOrder = li.SortOrder
        }).ToList();

        var subtotal = lineItems.Sum(li => li.Total);

        var quote = new Quote
        {
            Id = GenerateCuid(),
            QuoteNumber = quoteNumber,
            JobId = dto.JobId,
            CustomerId = dto.CustomerId,
            Status = QuoteStatus.Draft,
            PaymentStatus = "unpaid",
            Subtotal = subtotal,
            VatRate = 0,
            VatAmount = 0,
            Total = subtotal,
            DepositAmount = dto.DepositAmount,
            BalanceAmount = dto.DepositAmount.HasValue ? subtotal - dto.DepositAmount.Value : null,
            AmountPaid = 0,
            ExpiryDate = dto.ExpiryDate,
            Notes = dto.Notes,
            CreatedBy = createdBy,
            LineItems = lineItems
        };

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task<List<Quote>> GetQuotesAsync(QuoteStatus? status = null)
    {
        var query = _db.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .Include(q => q.QuotePayments)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        return await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
    }

    public async Task<Quote?> GetQuoteAsync(string id)
    {
        return await _db.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .Include(q => q.Job)
            .Include(q => q.QuotePayments)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quote?> ApproveQuoteAsync(string id, string? paymentReference = null)
    {
        var quote = await _db.Quotes.FindAsync(id);
        if (quote == null) return null;

        quote.Status = QuoteStatus.Approved;
        quote.PaymentReference = paymentReference ?? $"RAMS-Q-{quote.QuoteNumber}";
        quote.ApprovedAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task<Quote?> RejectQuoteAsync(string id, string reason)
    {
        var quote = await _db.Quotes.FindAsync(id);
        if (quote == null) return null;

        quote.Status = QuoteStatus.Rejected;
        quote.RejectionReason = reason;
        quote.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task<QuotePayment?> RecordQuotePaymentAsync(string quoteId, decimal amount, string reference, string? payerName, string? bankNotificationId)
    {
        var quote = await _db.Quotes
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote == null) return null;

        var payment = new QuotePayment
        {
            Id = GenerateCuid(),
            QuoteId = quoteId,
            Amount = amount,
            Reference = reference,
            PayerName = payerName,
            BankNotificationId = bankNotificationId,
            RecordedAt = DateTime.UtcNow
        };

        _db.QuotePayments.Add(payment);

        quote.AmountPaid += amount;
        quote.PaymentStatus = quote.AmountPaid >= quote.Total ? "paid" : "partial";

        if (quote.AmountPaid >= quote.Total)
        {
            quote.Status = QuoteStatus.Paid;
            quote.PaidAt = DateTime.UtcNow;
        }

        quote.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return payment;
    }

    public async Task<Invoice?> ConvertToInvoiceAsync(string quoteId)
    {
        var quote = await _db.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null || quote.AmountPaid < quote.Total) return null;

        var invoiceNumber = await GenerateInvoiceNumber();

        var invoice = new Invoice
        {
            Id = GenerateCuid(),
            InvoiceNumber = invoiceNumber,
            JobId = quote.JobId,
            CustomerId = quote.CustomerId,
            Status = InvoiceStatus.Paid,
            Subtotal = quote.Subtotal,
            VatRate = quote.VatRate,
            VatAmount = quote.VatAmount,
            Total = quote.Total,
            PaidAt = quote.PaidAt,
            DueDate = DateTime.UtcNow,
            Notes = $"Converted from quote {quote.QuoteNumber}. Reference: {quote.PaymentReference}",
            CreatedBy = quote.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LineItems = quote.LineItems.Select(li => new InvoiceLineItem
            {
                Id = GenerateCuid(),
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                Total = li.Total,
                Category = li.Category,
                SortOrder = li.SortOrder
            }).ToList()
        };

        _db.Invoices.Add(invoice);

        quote.Status = QuoteStatus.ConvertedToInvoice;
        quote.ConvertedInvoiceId = invoice.Id;
        quote.ConvertedToInvoiceAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return invoice;
    }

    private async Task<string> GenerateQuoteNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"Q-{year}-";

        var last = await _db.Quotes
            .Where(q => q.QuoteNumber.StartsWith(prefix))
            .OrderByDescending(q => q.QuoteNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (last != null)
        {
            var lastStr = last.QuoteNumber.Replace(prefix, "");
            if (int.TryParse(lastStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var last = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (last != null)
        {
            var lastStr = last.InvoiceNumber.Replace(prefix, "");
            if (int.TryParse(lastStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private static string GenerateCuid()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        var random = Guid.NewGuid().ToString("N")[..12];
        return $"c{timestamp}{random}";
    }
}
