using RamsElec.Shared.Enums;

namespace RamsElec.Shared.Models;

public class Payment
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? PayerName { get; set; }
    public decimal? MatchedConfidence { get; set; }
    public string? MatchedBy { get; set; }
    public string? BankNotification { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice? Invoice { get; set; }
}
