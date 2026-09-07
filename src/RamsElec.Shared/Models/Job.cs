namespace RamsElec.Shared.Models;

public class Job
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "open";
    public string CustomerId { get; set; } = string.Empty;
    public string? TechnicianId { get; set; }
    public string? ServiceType { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public List<Invoice> Invoices { get; set; } = [];
}
