using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.Models;

namespace RamsElec.App.ViewModels;

public partial class CompanySettingsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public CompanySettingsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private CompanyInfo _company = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _vatRate = "0.15";

    [RelayCommand]
    private async Task LoadCompanyAsync()
    {
        IsLoading = true;
        try
        {
            var company = await _apiClient.GetCompanyInfoAsync();
            Company = company ?? new CompanyInfo();
            VatRate = (Company.VatRate * 100).ToString("0.00");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveCompanyAsync()
    {
        if (decimal.TryParse(VatRate, out var vatPercent))
            Company.VatRate = vatPercent / 100;

        IsSaving = true;
        try
        {
            var saved = await _apiClient.SaveCompanyInfoAsync(Company);
            if (saved != null)
            {
                await Shell.Current.DisplayAlert("Saved", "Company details saved", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to save", "OK");
            }
        }
        finally
        {
            IsSaving = false;
        }
    }
}
