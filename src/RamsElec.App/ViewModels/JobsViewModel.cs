using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.Models;

namespace RamsElec.App.ViewModels;

public partial class JobsViewModel : ObservableObject
{
    private readonly LocalDatabase _localDb;

    public JobsViewModel(LocalDatabase localDb)
    {
        _localDb = localDb;
    }

    public ObservableCollection<Job> Jobs { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterStatus = "all";

    [RelayCommand]
    private async Task LoadJobsAsync()
    {
        IsLoading = true;
        try
        {
            var jobs = await _localDb.GetJobsAsync();
            Jobs.Clear();

            var filtered = FilterStatus == "all"
                ? jobs
                : jobs.Where(j => j.Status == FilterStatus).ToList();

            foreach (var job in filtered)
                Jobs.Add(job);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
