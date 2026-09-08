using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RamsElec.Api.Data;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Services;

public class OverdueInvoiceService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OverdueInvoiceService> _logger;

    public OverdueInvoiceService(IServiceProvider services, ILogger<OverdueInvoiceService> logger)
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
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var overdue = await db.Invoices
                    .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var invoice in overdue)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                    invoice.UpdatedAt = DateTime.UtcNow;
                }

                if (overdue.Any())
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Marked {Count} invoices as overdue", overdue.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue check failed");
            }

            // Run every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
