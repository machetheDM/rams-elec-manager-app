using SQLite;
using RamsElec.Shared.Models;

namespace RamsElec.App.Services;

public class LocalDatabase
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;

    public LocalDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "ramselec.db3");
    }

    private async Task<SQLiteAsyncConnection> GetConnection()
    {
        if (_db != null) return _db;

        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Invoice>();
        await _db.CreateTableAsync<InvoiceLineItem>();
        await _db.CreateTableAsync<Customer>();
        await _db.CreateTableAsync<Job>();
        return _db;
    }

    // Customers
    public async Task<List<Customer>> GetCustomersAsync()
    {
        var db = await GetConnection();
        return await db.Table<Customer>().ToListAsync();
    }

    public async Task SaveCustomerAsync(Customer customer)
    {
        var db = await GetConnection();
        await db.InsertOrReplaceAsync(customer);
    }

    public async Task SaveCustomersAsync(List<Customer> customers)
    {
        var db = await GetConnection();
        await db.RunInTransactionAsync(conn =>
        {
            foreach (var c in customers)
                conn.InsertOrReplace(c);
        });
    }

    // Jobs
    public async Task<List<Job>> GetJobsAsync()
    {
        var db = await GetConnection();
        return await db.Table<Job>().ToListAsync();
    }

    public async Task<List<Job>> GetCompletedJobsAsync()
    {
        var db = await GetConnection();
        return await db.Table<Job>().Where(j => j.Status == "complete").ToListAsync();
    }

    public async Task SaveJobsAsync(List<Job> jobs)
    {
        var db = await GetConnection();
        await db.RunInTransactionAsync(conn =>
        {
            foreach (var j in jobs)
                conn.InsertOrReplace(j);
        });
    }

    // Invoices
    public async Task<List<Invoice>> GetInvoicesAsync()
    {
        var db = await GetConnection();
        return await db.Table<Invoice>().OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task SaveInvoiceAsync(Invoice invoice)
    {
        var db = await GetConnection();
        await db.InsertOrReplaceAsync(invoice);
    }

    // Sync metadata
    public async Task<DateTime?> GetLastSyncedAtAsync()
    {
        var db = await GetConnection();
        try
        {
            var result = await db.ExecuteScalarAsync<string>(
                "SELECT value FROM sync_meta WHERE key = 'last_synced_at'");
            return result != null ? DateTime.Parse(result) : null;
        }
        catch
        {
            await db.ExecuteAsync(
                "CREATE TABLE IF NOT EXISTS sync_meta (key TEXT PRIMARY KEY, value TEXT)");
            return null;
        }
    }

    public async Task SetLastSyncedAtAsync(DateTime timestamp)
    {
        var db = await GetConnection();
        await db.ExecuteAsync(
            "CREATE TABLE IF NOT EXISTS sync_meta (key TEXT PRIMARY KEY, value TEXT)");
        await db.ExecuteAsync(
            "INSERT OR REPLACE INTO sync_meta (key, value) VALUES ('last_synced_at', ?)",
            timestamp.ToString("O"));
    }
}
