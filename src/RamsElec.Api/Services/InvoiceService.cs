using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class InvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Invoice> CreateInvoice(CreateInvoiceDto dto, string createdBy)
    {
        var invoiceNumber = await GenerateInvoiceNumber();

        var lineItems = dto.LineItems.Select(li => new InvoiceLineItem
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

        var invoice = new Invoice
        {
            Id = GenerateCuid(),
            InvoiceNumber = invoiceNumber,
            JobId = dto.JobId,
            CustomerId = dto.CustomerId,
            QuoteId = dto.QuoteId,
            Status = InvoiceStatus.Draft,
            Subtotal = subtotal,
            VatRate = 0,
            VatAmount = 0,
            Total = subtotal,
            DueDate = dto.DueDate,
            Notes = dto.Notes,
            CreatedBy = createdBy,
            LineItems = lineItems
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return invoice;
    }

    public async Task<List<Invoice>> GetInvoices(InvoiceStatus? status = null)
    {
        var query = _db.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task<Invoice?> GetInvoice(string id)
    {
        return await _db.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .Include(i => i.Job)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> MarkAsSent(string id, DeliveryChannel channel)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice == null) return null;

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentVia = channel;
        invoice.SentAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> MarkAsPaid(string id)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice == null) return null;

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return invoice;
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastInvoice = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private static string GenerateCuid()
    {
        // Simple CUID-like ID compatible with Prisma's cuid() format
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        var random = Guid.NewGuid().ToString("N")[..12];
        return $"c{timestamp}{random}";
    }
}
