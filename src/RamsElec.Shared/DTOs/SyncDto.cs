namespace RamsElec.Shared.DTOs;

public class SyncRequestDto
{
    public DateTime? LastSyncedAt { get; set; }
}

public class SyncResponseDto
{
    public List<CustomerSyncDto> Customers { get; set; } = [];
    public List<JobSyncDto> Jobs { get; set; } = [];
    public DateTime ServerTimestamp { get; set; }
}

public class CustomerSyncDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class JobSyncDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? TechnicianId { get; set; }
    public string? ServiceType { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime UpdatedAt { get; set; }
}
