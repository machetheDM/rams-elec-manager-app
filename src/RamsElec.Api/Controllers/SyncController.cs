using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.DTOs;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _db;

    public SyncController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] SyncRequestDto dto)
    {
        var lastSynced = dto.LastSyncedAt ?? DateTime.MinValue;

        var customers = await _db.Customers
            .Where(c => c.UpdatedAt > lastSynced)
            .ToListAsync();

        var jobs = await _db.Jobs
            .Where(j => j.UpdatedAt > lastSynced)
            .Include(j => j.Customer)
            .ToListAsync();

        var response = new SyncResponseDto
        {
            Customers = customers.Select(c => new CustomerSyncDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Province = c.Province,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            Jobs = jobs.Select(j => new JobSyncDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Status = j.Status,
                CustomerId = j.CustomerId,
                CustomerName = j.Customer?.Name,
                TechnicianId = j.TechnicianId,
                ServiceType = j.ServiceType,
                EstimatedCost = j.EstimatedCost,
                ActualCost = j.ActualCost,
                ScheduledDate = j.ScheduledDate,
                CompletedDate = j.CompletedDate,
                UpdatedAt = j.UpdatedAt
            }).ToList(),
            ServerTimestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}
