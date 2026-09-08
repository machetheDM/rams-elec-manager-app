namespace RamsElec.Shared.Models;

public class BankPaymentNotification
{
    public string Id { get; set; } = string.Empty;
    public string SourceEmailId { get; set; } = string.Empty;
    public string? RawSubject { get; set; }
    public string? RawBody { get; set; }
    public string? PayerName { get; set; }
    public string? Reference { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? BankName { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
