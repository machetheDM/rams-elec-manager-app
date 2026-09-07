namespace RamsElec.Shared.DTOs;

public class CreateInvoiceDto
{
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? QuoteId { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateLineItemDto> LineItems { get; set; } = [];
}

public class CreateLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Category { get; set; } = "service";
    public int SortOrder { get; set; }
}
