using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class SendQuoteDto
{
    public string QuoteId { get; set; } = string.Empty;
    public DeliveryChannel Channel { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public string? ApprovalToken { get; set; }
}
