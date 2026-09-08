using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class QuoteDto
{
    public string Id { get; set; } = string.Empty;
    public string QuoteNumber { get; set; } = string.Empty;
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public QuoteStatus Status { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<QuoteLineItemDto> LineItems { get; set; } = [];
    public List<QuotePaymentDto> QuotePayments { get; set; } = [];
}

public class QuoteLineItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string Category { get; set; } = "service";
    public int SortOrder { get; set; }
}

public class QuotePaymentDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? PayerName { get; set; }
    public DateTime RecordedAt { get; set; }
}
