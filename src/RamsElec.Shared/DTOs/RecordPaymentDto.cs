using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class RecordPaymentDto
{
    public string InvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? PayerName { get; set; }
}
