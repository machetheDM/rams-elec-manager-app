using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] string? status)
    {
        var query = _db.Jobs.Include(j => j.Customer).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(j => j.Status == status);

        var jobs = await query.ToListAsync();
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(string id)
    {
        var job = await _db.Jobs
            .Include(j => j.Customer)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] Job job)
    {
        job.Id = job.Id != string.Empty ? job.Id : GenerateCuid();
        job.CreatedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    private static string GenerateCuid()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        var random = Guid.NewGuid().ToString("N")[..12];
        return $"c{timestamp}{random}";
    }
}
