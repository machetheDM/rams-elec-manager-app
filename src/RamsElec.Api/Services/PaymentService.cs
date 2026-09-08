using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class PaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Payment?> RecordPaymentAsync(RecordPaymentDto dto)
    {
        var invoice = await _db.Invoices.FindAsync(dto.InvoiceId);
        if (invoice == null) return null;

        var payment = new Payment
        {
            Id = GenerateCuid(),
            InvoiceId = dto.InvoiceId,
            Amount = dto.Amount,
            Method = dto.Method,
            Reference = dto.Reference,
            PayerName = dto.PayerName,
            RecordedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);

        var totalPaid = await _db.Payments
            .Where(p => p.InvoiceId == dto.InvoiceId)
            .SumAsync(p => p.Amount);

        invoice.PaidAt = DateTime.UtcNow;

        if (totalPaid >= invoice.Total)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return payment;
    }

    public async Task<List<Payment>> GetPaymentsForInvoiceAsync(string invoiceId)
    {
        return await _db.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.RecordedAt)
            .ToListAsync();
    }

    private static string GenerateCuid()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        var random = Guid.NewGuid().ToString("N")[..12];
        return $"c{timestamp}{random}";
    }
}
