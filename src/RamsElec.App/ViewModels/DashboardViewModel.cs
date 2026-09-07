using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;

namespace RamsElec.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly LocalDatabase _localDb;
    private readonly SyncService _syncService;
    private readonly AuthService _authService;

    public DashboardViewModel(LocalDatabase localDb, SyncService syncService, AuthService authService)
    {
        _localDb = localDb;
        _syncService = syncService;
        _authService = authService;
    }

    [ObservableProperty]
    private int _totalJobs;

    [ObservableProperty]
    private int _openJobs;

    [ObservableProperty]
    private int _completedJobs;

    [ObservableProperty]
    private int _totalInvoices;

    [ObservableProperty]
    private string _welcomeMessage = "Welcome";

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _lastSynced = "Never";

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        WelcomeMessage = $"Welcome, {_authService.DisplayName ?? "Manager"}";

        var jobs = await _localDb.GetJobsAsync();
        TotalJobs = jobs.Count;
        OpenJobs = jobs.Count(j => j.Status is "open" or "assigned" or "in_progress");
        CompletedJobs = jobs.Count(j => j.Status == "complete");

        var invoices = await _localDb.GetInvoicesAsync();
        TotalInvoices = invoices.Count;

        var lastSync = await _localDb.GetLastSyncedAtAsync();
        LastSynced = lastSync?.ToString("dd/MM/yyyy HH:mm") ?? "Never";
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        IsSyncing = true;
        try
        {
            await _syncService.SyncAsync();
            await LoadDataAsync();
        }
        finally
        {
            IsSyncing = false;
        }
    }
}
