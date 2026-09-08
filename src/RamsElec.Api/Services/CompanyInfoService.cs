using Microsoft.EntityFrameworkCore;
using RamsElec.Api.Data;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class CompanyInfoService
{
    private readonly AppDbContext _db;

    public CompanyInfoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CompanyInfo> GetOrCreateAsync()
    {
        var existing = await _db.Set<CompanyInfo>().FirstOrDefaultAsync();
        if (existing != null) return existing;

        var created = new CompanyInfo();
        _db.Set<CompanyInfo>().Add(created);
        await _db.SaveChangesAsync();
        return created;
    }

    public async Task<CompanyInfo> UpdateAsync(CompanyInfo info)
    {
        var existing = await _db.Set<CompanyInfo>().FirstOrDefaultAsync();
        if (existing == null)
        {
            _db.Set<CompanyInfo>().Add(info);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(info);
        }

        await _db.SaveChangesAsync();
        return existing ?? info;
    }
}
