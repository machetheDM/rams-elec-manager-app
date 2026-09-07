namespace RamsElec.Shared.DTOs;

public class DashboardAnalyticsDto
{
    public int TotalJobs { get; set; }
    public int JobsThisMonth { get; set; }
    public int OpenJobs { get; set; }
    public int CompletedJobs { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int OverdueInvoices { get; set; }
    public int PendingPayments { get; set; }
    public List<JobsByStatusDto> JobsByStatus { get; set; } = [];
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = [];
}

public class JobsByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RevenueByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int InvoiceCount { get; set; }
}
