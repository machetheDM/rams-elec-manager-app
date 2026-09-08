namespace RamsElec.Shared.DTOs;

public class CreateQuoteDto
{
    public string? JobId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public decimal? DepositAmount { get; set; }
    public List<CreateQuoteLineItemDto> LineItems { get; set; } = [];
}

public class CreateQuoteLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Category { get; set; } = "service";
    public int SortOrder { get; set; }
}
