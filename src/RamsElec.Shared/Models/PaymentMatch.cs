using RamsElec.Shared.Enums;

namespace RamsElec.Shared.Models;

public class PaymentMatch
{
    public string Id { get; set; } = string.Empty;
    public string? InvoiceId { get; set; }
    public string? BankPaymentId { get; set; }
    public decimal Confidence { get; set; }
    public string Status { get; set; } = "pending"; // pending | approved | rejected | auto_processed
    public string? MatchedBy { get; set; }
    public string? ReviewReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Invoice? Invoice { get; set; }
    public BankPaymentNotification? BankPayment { get; set; }
}
