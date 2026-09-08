using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

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

    [ObservableProperty]
    private ObservableCollection<Payment> _payments = [];

    [RelayCommand]
    private async Task LoadInvoiceAsync()
    {
        IsLoading = true;
        try
        {
            Invoice = await _apiClient.GetInvoiceAsync(InvoiceId);

            Payments.Clear();
            var payments = await _apiClient.GetPaymentsForInvoiceAsync(InvoiceId);
            foreach (var payment in payments)
                Payments.Add(payment);
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

        var method = await Shell.Current.DisplayActionSheet("Payment method", "Cancel", null, "EFT", "Cash");
        if (method is null || method == "Cancel") return;

        var methodEnum = method == "EFT" ? PaymentMethod.Eft : PaymentMethod.Cash;

        var reference = await Shell.Current.DisplayPromptAsync("Payment",
            "Enter payment reference (optional):");

        var dto = new RecordPaymentDto
        {
            InvoiceId = InvoiceId,
            Amount = parsedAmount,
            Method = methodEnum,
            Reference = reference
        };

        var success = await _apiClient.RecordPaymentAsync(dto);
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

    [RelayCommand]
    private async Task RecordFnbPaymentAsync()
    {
        if (Invoice == null) return;

        var approvalCode = await Shell.Current.DisplayPromptAsync("FNB SpeedPoint", "Enter approval code:");
        if (string.IsNullOrWhiteSpace(approvalCode)) return;

        var lastFour = await Shell.Current.DisplayPromptAsync("FNB SpeedPoint", "Enter last 4 card digits:", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(lastFour)) return;

        var amount = await Shell.Current.DisplayPromptAsync("FNB SpeedPoint",
            "Enter payment amount:", initialValue: Invoice.Total.ToString("0.00"), keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(amount)) return;

        if (!decimal.TryParse(amount, out var parsedAmount))
        {
            await Shell.Current.DisplayAlert("Error", "Invalid amount", "OK");
            return;
        }

        var dto = new FnbPaymentDto
        {
            InvoiceId = InvoiceId,
            Amount = parsedAmount,
            ApprovalCode = approvalCode,
            LastFourDigits = lastFour
        };

        var success = await _apiClient.RecordFnbPaymentAsync(dto);
        if (success)
        {
            await Shell.Current.DisplayAlert("Paid", "FNB payment recorded", "OK");
            await LoadInvoiceAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to record FNB payment", "OK");
        }
    }
}
