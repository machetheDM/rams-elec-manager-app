namespace RamsElec.Shared.Models;

public class InvoiceLineItem
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string Category { get; set; } = "service";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
