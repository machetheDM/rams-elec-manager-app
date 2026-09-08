using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public AnalyticsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private DashboardAnalyticsDto? _analytics;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private LiveChartsCore.ISeries[] _revenueSeries = [];

    [ObservableProperty]
    private LiveChartsCore.ISeries[] _jobsByStatusSeries = [];

    [ObservableProperty]
    private LiveChartsCore.SkiaSharpView.Axis[] _revenueXAxes = [];

    [ObservableProperty]
    private LiveChartsCore.SkiaSharpView.Axis[] _revenueYAxes = [];

    [RelayCommand]
    private async Task LoadAnalyticsAsync()
    {
        IsLoading = true;
        try
        {
            Analytics = await _apiClient.GetDashboardAnalyticsAsync();
            BuildCharts();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildCharts()
    {
        if (Analytics == null) return;

        var revenueValues = new LiveChartsCore.Defaults.ObservableValue[Analytics.RevenueByMonth.Count];
        var monthLabels = new string[Analytics.RevenueByMonth.Count];
        for (int i = 0; i < Analytics.RevenueByMonth.Count; i++)
        {
            revenueValues[i] = new LiveChartsCore.Defaults.ObservableValue((double)Analytics.RevenueByMonth[i].Revenue);
            monthLabels[i] = Analytics.RevenueByMonth[i].Month;
        }

        RevenueSeries =
        [
            new LiveChartsCore.SkiaSharpView.ColumnSeries<LiveChartsCore.Defaults.ObservableValue>
            {
                Values = revenueValues,
                Name = "Revenue",
                DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColors.White),
                DataLabelsSize = 10,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
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

        var statusValues = new LiveChartsCore.Defaults.ObservableValue[Analytics.JobsByStatus.Count];
        var statusLabels = new string[Analytics.JobsByStatus.Count];
        for (int i = 0; i < Analytics.JobsByStatus.Count; i++)
        {
            statusValues[i] = new LiveChartsCore.Defaults.ObservableValue(Analytics.JobsByStatus[i].Count);
            statusLabels[i] = Analytics.JobsByStatus[i].Status;
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
