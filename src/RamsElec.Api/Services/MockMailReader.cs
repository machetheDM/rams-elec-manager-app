namespace RamsElec.Api.Services;

public class MockMailReader : IMailReader
{
    public Task<List<EmailMessage>> GetUnreadPaymentNotificationsAsync(DateTime? since = null)
    {
        // Dev-only mock for testing payment matching without M365
        return Task.FromResult(new List<EmailMessage>());
    }
}
