using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.ViewModels;

[QueryProperty("QuoteId", "QuoteId")]
public partial class QuoteDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public QuoteDetailViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private string _quoteId = string.Empty;

    [ObservableProperty]
    private QuoteDto? _quote;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadQuoteAsync()
    {
        IsLoading = true;
        try
        {
            Quote = await _apiClient.GetQuoteAsync(QuoteId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApproveAsync()
    {
        if (Quote == null) return;
        var result = await _apiClient.ApproveQuoteAsync(QuoteId);
        if (result != null)
        {
            await Shell.Current.DisplayAlert("Approved", $"Payment reference: {result.PaymentReference}", "OK");
            await LoadQuoteAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to approve quote", "OK");
        }
    }

    [RelayCommand]
    private async Task RejectAsync()
    {
        if (Quote == null) return;
        var reason = await Shell.Current.DisplayPromptAsync("Reject Quote", "Reason:");
        if (string.IsNullOrWhiteSpace(reason)) return;

        var result = await _apiClient.RejectQuoteAsync(QuoteId, reason);
        if (result != null)
        {
            await Shell.Current.DisplayAlert("Rejected", "Quote rejected", "OK");
            await LoadQuoteAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to reject quote", "OK");
        }
    }

    [RelayCommand]
    private async Task ConvertToInvoiceAsync()
    {
        if (Quote == null) return;
        var invoice = await _apiClient.ConvertQuoteToInvoiceAsync(QuoteId);
        if (invoice != null)
        {
            await Shell.Current.DisplayAlert("Converted", $"Invoice {invoice.InvoiceNumber} created", "OK");
            await LoadQuoteAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Quote not fully paid yet", "OK");
        }
    }
}
