using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class SendInvoiceDto
{
    public string InvoiceId { get; set; } = string.Empty;
    public DeliveryChannel Channel { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
}
