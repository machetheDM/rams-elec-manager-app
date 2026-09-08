using System.Text.RegularExpressions;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public interface IBankEmailParser
{
    bool CanParse(EmailMessage message);
    BankPaymentNotification? Parse(EmailMessage message);
}

public class FnbPaymentNotificationParser : IBankEmailParser
{
    public bool CanParse(EmailMessage message)
    {
        return message.From.Contains("fnb.co.za", StringComparison.OrdinalIgnoreCase) ||
               message.Subject.Contains("FNB", StringComparison.OrdinalIgnoreCase);
    }

    public BankPaymentNotification? Parse(EmailMessage message)
    {
        // IMPORTANT: This parser is a design scaffold. The exact FNB email format
        // must be verified with real samples before processing live payments.
        var amount = ExtractAmount(message.Body);
        if (!amount.HasValue) return null;

        var reference = ExtractReference(message.Body) ?? ExtractReference(message.Subject);
        var name = ExtractPayerName(message.Body);
        var date = ExtractDate(message.Body) ?? message.ReceivedDateTime;

        return new BankPaymentNotification
        {
            Id = $"fnb-{message.Id}",
            SourceEmailId = message.Id,
            RawSubject = message.Subject,
            RawBody = message.Body,
            PayerName = name,
            Reference = reference,
            Amount = amount,
            PaymentDate = date,
            BankName = "FNB",
            ReceivedAt = message.ReceivedDateTime
        };
    }

    private static decimal? ExtractAmount(string text)
    {
        // Example patterns that must be verified against real FNB emails
        var match = Regex.Match(text, @"(?:amount|paid|payment)[:\s]+R?\s*([\d\s]+(?:\.\d{2})?)", RegexOptions.IgnoreCase);
        if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(" ", ""), out var value))
            return value;

        match = Regex.Match(text, @"R\s*([\d\s,]+\.?\d{0,2})", RegexOptions.IgnoreCase);
        if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(" ", "").Replace(",", ""), out var fallback))
            return fallback;

        return null;
    }

    private static string? ExtractReference(string text)
    {
        var match = Regex.Match(text, @"(?:reference|ref)[:\s]+(\S+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractPayerName(string text)
    {
        var match = Regex.Match(text, @"(?:from|payer|customer)[:\s]+([\w\s]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static DateTime? ExtractDate(string text)
    {
        var match = Regex.Match(text, @"(\d{2}/\d{2}/\d{4})");
        if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        match = Regex.Match(text, @"(\d{4}-\d{2}-\d{2})");
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var fallback))
            return fallback;

        return null;
    }
}
