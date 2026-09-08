using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;

namespace RamsElec.Api.Services;

public class AnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync()
    {
        var today = DateTime.UtcNow;
        var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var jobs = await _db.Jobs.ToListAsync();
        var invoices = await _db.Invoices.ToListAsync();

        var totalJobs = jobs.Count;
        var openJobs = jobs.Count(j => j.Status is "open" or "assigned" or "in_progress");
        var completedJobs = jobs.Count(j => j.Status == "complete");

        var jobsThisMonth = jobs.Count(j => j.CreatedAt >= startOfMonth);
        var revenueThisMonth = invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= startOfMonth)
            .Sum(i => i.Total);

        var totalOutstanding = invoices
            .Where(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.Draft)
            .Sum(i => i.Total);

        var overdueInvoices = invoices
            .Count(i => i.Status == InvoiceStatus.Sent && i.DueDate < today);

        var pendingPayments = invoices
            .Count(i => i.Status is InvoiceStatus.Sent or InvoiceStatus.PartiallyPaid);

        var statusMap = new Dictionary<string, int>();
        foreach (var job in jobs)
            statusMap[job.Status] = statusMap.GetValueOrDefault(job.Status) + 1;

        var revenueByMonth = new Dictionary<string, (decimal revenue, int count)>();
        for (int i = 5; i >= 0; i--)
        {
            var month = today.AddMonths(-i);
            var key = month.ToString("MMM yyyy");
            revenueByMonth[key] = (0m, 0);
        }

        foreach (var inv in invoices.Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue))
        {
            var key = inv.PaidAt!.Value.ToString("MMM yyyy");
            if (revenueByMonth.ContainsKey(key))
            {
                var (rev, count) = revenueByMonth[key];
                revenueByMonth[key] = (rev + inv.Total, count + 1);
            }
        }

        return new DashboardAnalyticsDto
        {
            TotalJobs = totalJobs,
            JobsThisMonth = jobsThisMonth,
            OpenJobs = openJobs,
            CompletedJobs = completedJobs,
            RevenueThisMonth = revenueThisMonth,
            TotalOutstanding = totalOutstanding,
            OverdueInvoices = overdueInvoices,
            PendingPayments = pendingPayments,
            JobsByStatus = statusMap.Select(s => new JobsByStatusDto { Status = s.Key, Count = s.Value }).ToList(),
            RevenueByMonth = revenueByMonth.Select(kv => new RevenueByMonthDto
            {
                Month = kv.Key,
                Revenue = kv.Value.revenue,
                InvoiceCount = kv.Value.count
            }).ToList()
        };
    }
}
