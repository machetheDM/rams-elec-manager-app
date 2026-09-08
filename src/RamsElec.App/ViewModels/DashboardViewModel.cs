using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;

namespace RamsElec.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly LocalDatabase _localDb;
    private readonly SyncService _syncService;
    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;

    public DashboardViewModel(LocalDatabase localDb, SyncService syncService, AuthService authService, ApiClient apiClient)
    {
        _localDb = localDb;
        _syncService = syncService;
        _authService = authService;
        _apiClient = apiClient;
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
    private decimal _revenueThisMonth;

    [ObservableProperty]
    private string _welcomeMessage = "Welcome";

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _lastSynced = "Never";

    [ObservableProperty]
    private LiveChartsCore.ISeries[] _revenueSeries = [];

    [ObservableProperty]
    private LiveChartsCore.ISeries[] _jobsByStatusSeries = [];

    [ObservableProperty]
    private LiveChartsCore.SkiaSharpView.Axis[] _revenueXAxes = [];

    [ObservableProperty]
    private LiveChartsCore.SkiaSharpView.Axis[] _revenueYAxes = [];

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

        try
        {
            var analytics = await _apiClient.GetDashboardAnalyticsAsync();
            if (analytics != null)
            {
                TotalJobs = analytics.TotalJobs;
                OpenJobs = analytics.OpenJobs;
                CompletedJobs = analytics.CompletedJobs;
                RevenueThisMonth = analytics.RevenueThisMonth;

                var revenueValues = new LiveChartsCore.Defaults.ObservableValue[analytics.RevenueByMonth.Count];
                var monthLabels = new string[analytics.RevenueByMonth.Count];
                for (int i = 0; i < analytics.RevenueByMonth.Count; i++)
                {
                    revenueValues[i] = new LiveChartsCore.Defaults.ObservableValue((double)analytics.RevenueByMonth[i].Revenue);
                    monthLabels[i] = analytics.RevenueByMonth[i].Month;
                }

                RevenueSeries =
                [
                    new LiveChartsCore.SkiaSharpView.ColumnSeries<LiveChartsCore.Defaults.ObservableValue>
                    {
                        Values = revenueValues,
                        Name = "Revenue",
                        DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColors.White),
                        DataLabelsSize = 10,
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                        YToolTipLabelFormatter = (point) => $"R{point.Model!.Value:N2}"
                    }
                ];

                RevenueXAxes =
                [
                    new LiveChartsCore.SkiaSharpView.Axis { Labels = monthLabels, LabelsRotation = 45 }
                ];

                RevenueYAxes =
                [
                    new LiveChartsCore.SkiaSharpView.Axis { Labeler = (value) => $"R{value:N0}" }
                ];

                var statusValues = new LiveChartsCore.Defaults.ObservableValue[analytics.JobsByStatus.Count];
                var statusLabels = new string[analytics.JobsByStatus.Count];
                for (int i = 0; i < analytics.JobsByStatus.Count; i++)
                {
                    statusValues[i] = new LiveChartsCore.Defaults.ObservableValue(analytics.JobsByStatus[i].Count);
                    statusLabels[i] = analytics.JobsByStatus[i].Status;
                }

                JobsByStatusSeries =
                [
                    new LiveChartsCore.SkiaSharpView.PieSeries<LiveChartsCore.Defaults.ObservableValue>
                    {
                        Values = statusValues,
                        Name = "Jobs",
                        DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColors.White),
                        DataLabelsSize = 10,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                        DataLabelsFormatter = (point) => $"{point.Model!.Value}"
                    }
                ];
            }
        }
        catch
        {
            // API may be unavailable; keep locally cached counts
        }
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
