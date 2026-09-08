namespace RamsElec.Shared.Models;

public class QuotePayment
{
    public string Id { get; set; } = string.Empty;
    public string QuoteId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? PayerName { get; set; }
    public string? BankNotificationId { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Quote? Quote { get; set; }
}
