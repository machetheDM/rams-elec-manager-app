namespace RamsElec.Api.Services;

public class PaymentAgentHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PaymentAgentHostedService> _logger;

    public PaymentAgentHostedService(IServiceProvider services, ILogger<PaymentAgentHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var mailReader = scope.ServiceProvider.GetRequiredService<IMailReader>();
                var parser = scope.ServiceProvider.GetRequiredService<IBankEmailParser>();
                var matcher = scope.ServiceProvider.GetRequiredService<PaymentMatchingService>();

                var messages = await mailReader.GetUnreadPaymentNotificationsAsync(DateTime.UtcNow.AddDays(-7));

                foreach (var msg in messages)
                {
                    if (!parser.CanParse(msg)) continue;

                    var notification = parser.Parse(msg);
                    if (notification != null)
                    {
                        await matcher.EvaluateAsync(notification);
                    }
                }

                _logger.LogInformation("Payment agent scanned {Count} messages", messages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment agent failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
