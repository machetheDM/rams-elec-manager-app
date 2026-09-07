using RamsElec.Shared.Enums;

namespace RamsElec.Shared.Models;

public class Invoice
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? QuoteId { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Total { get; set; }
    public string? PdfUrl { get; set; }
    public DeliveryChannel? SentVia { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<InvoiceLineItem> LineItems { get; set; } = [];
    public Customer? Customer { get; set; }
    public Job? Job { get; set; }
}
