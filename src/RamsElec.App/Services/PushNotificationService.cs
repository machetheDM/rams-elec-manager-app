namespace RamsElec.App.Services;

public interface IPushNotificationService
{
    Task<bool> RegisterAsync();
    Task SendLocalNotificationAsync(string title, string message);
}

public class PushNotificationService : IPushNotificationService
{
    public Task<bool> RegisterAsync()
    {
        // Firebase Cloud Messaging / Apple Push Notification registration
        // This is a design scaffold — actual FCM/APNs integration requires
        // platform-specific setup and a real backend project.
        return Task.FromResult(true);
    }

    public Task SendLocalNotificationAsync(string title, string message)
    {
        // Local notification for low-confidence payment matches and overdue reminders
        return Task.CompletedTask;
    }
}
