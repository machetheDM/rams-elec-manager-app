using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;

namespace RamsElec.App.ViewModels;

[QueryProperty("InvoiceId", "InvoiceId")]
public partial class InvoiceDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public InvoiceDetailViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private string _invoiceId = string.Empty;

    [ObservableProperty]
    private InvoiceDto? _invoice;

    [ObservableProperty]
    private byte[]? _pdfBytes;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSending;

    [RelayCommand]
    private async Task LoadInvoiceAsync()
    {
        IsLoading = true;
        try
        {
            Invoice = await _apiClient.GetInvoiceAsync(InvoiceId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        IsLoading = true;
        try
        {
            PdfBytes = await _apiClient.GetInvoicePdfAsync(InvoiceId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendInvoiceAsync(string channel)
    {
        if (Invoice == null) return;

        IsSending = true;
        try
        {
            var parsed = Enum.TryParse<DeliveryChannel>(channel, out var c) ? c : DeliveryChannel.Sms;
            var success = await _apiClient.SendInvoiceAsync(InvoiceId, parsed, Invoice.CustomerId);
            await Shell.Current.DisplayAlert("Sent",
                $"Invoice {Invoice.InvoiceNumber} queued for {channel}.", "OK");
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private async Task RecordPaymentAsync()
    {
        if (Invoice == null) return;

        var amount = await Shell.Current.DisplayPromptAsync("Payment",
            "Enter payment amount:", initialValue: Invoice.Total.ToString("0.00"), keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(amount)) return;

        if (!decimal.TryParse(amount, out var parsedAmount))
        {
            await Shell.Current.DisplayAlert("Error", "Invalid amount", "OK");
            return;
        }

        var success = await _apiClient.RecordPaymentAsync(InvoiceId, parsedAmount);
        if (success)
        {
            await Shell.Current.DisplayAlert("Paid", "Payment recorded", "OK");
            await LoadInvoiceAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to record payment", "OK");
        }
    }
}
