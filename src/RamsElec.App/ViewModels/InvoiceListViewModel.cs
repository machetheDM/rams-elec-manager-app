using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.App.Views;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.ViewModels;

public partial class InvoiceListViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public InvoiceListViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ObservableCollection<InvoiceDto> Invoices { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadInvoicesAsync()
    {
        IsLoading = true;
        try
        {
            var invoices = await _apiClient.GetInvoicesAsync();
            Invoices.Clear();
            foreach (var inv in invoices)
                Invoices.Add(inv);
        }
        catch
        {
            // Offline — show cached or empty
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToCreateAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.InvoiceCreatePage));
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(InvoiceDto invoice)
    {
        if (invoice is null) return;
        await Shell.Current.GoToAsync($"{nameof(InvoiceDetailPage)}?InvoiceId={invoice.Id}");
    }
}
