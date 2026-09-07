using RamsElec.Shared.DTOs;
using RamsElec.Shared.Models;

namespace RamsElec.App.Services;

public class SyncService
{
    private readonly ApiClient _apiClient;
    private readonly LocalDatabase _localDb;

    public SyncService(ApiClient apiClient, LocalDatabase localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public async Task<bool> SyncAsync()
    {
        if (!_apiClient.HasToken) return false;

        try
        {
            var lastSynced = await _localDb.GetLastSyncedAtAsync();
            var response = await _apiClient.SyncAsync(new SyncRequestDto
            {
                LastSyncedAt = lastSynced
            });

            if (response == null) return false;

            // Save customers locally
            var customers = response.Customers.Select(c => new Customer
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Province = c.Province,
                UpdatedAt = c.UpdatedAt
            }).ToList();
            await _localDb.SaveCustomersAsync(customers);

            // Save jobs locally
            var jobs = response.Jobs.Select(j => new Job
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Status = j.Status,
                CustomerId = j.CustomerId,
                TechnicianId = j.TechnicianId,
                ServiceType = j.ServiceType,
                EstimatedCost = j.EstimatedCost,
                ActualCost = j.ActualCost,
                ScheduledDate = j.ScheduledDate,
                CompletedDate = j.CompletedDate,
                UpdatedAt = j.UpdatedAt
            }).ToList();
            await _localDb.SaveJobsAsync(jobs);

            await _localDb.SetLastSyncedAtAsync(response.ServerTimestamp);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
