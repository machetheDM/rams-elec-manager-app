using RamsElec.Shared.Enums;

namespace RamsElec.Shared.Models;

public class Quote
{
    public string Id { get; set; } = string.Empty;
    public string QuoteNumber { get; set; } = string.Empty;
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public string PaymentStatus { get; set; } = "unpaid";
    public decimal Subtotal { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Total { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ConvertedToInvoiceAt { get; set; }
    public string? ConvertedInvoiceId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<QuoteLineItem> LineItems { get; set; } = [];
    public Customer? Customer { get; set; }
    public Job? Job { get; set; }
    public List<QuotePayment> QuotePayments { get; set; } = [];
}
