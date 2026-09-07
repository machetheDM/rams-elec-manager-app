using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class InvoiceDto
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? QuoteId { get; set; }
    public InvoiceStatus Status { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LineItemDto> LineItems { get; set; } = [];
}

public class LineItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string Category { get; set; } = "service";
    public int SortOrder { get; set; }
}
