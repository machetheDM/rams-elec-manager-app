using RamsElec.Shared.Enums;

namespace RamsElec.Shared.DTOs;

public class FnbPaymentDto
{
    public string InvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ApprovalCode { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? PayerName { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.FnbSpeedPoint;
}
