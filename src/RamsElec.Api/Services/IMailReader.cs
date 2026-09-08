namespace RamsElec.Api.Services;

public interface IMailReader
{
    Task<List<EmailMessage>> GetUnreadPaymentNotificationsAsync(DateTime? since = null);
}

public class EmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ReceivedDateTime { get; set; }
    public string From { get; set; } = string.Empty;
}
