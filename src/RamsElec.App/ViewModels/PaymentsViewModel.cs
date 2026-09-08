using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.Models;

namespace RamsElec.App.ViewModels;

public partial class PaymentsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public PaymentsViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ObservableCollection<PaymentMatch> PendingMatches { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadMatchesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading...";
        try
        {
            var matches = await _apiClient.GetPendingPaymentMatchesAsync();
            PendingMatches.Clear();
            foreach (var m in matches)
                PendingMatches.Add(m);

            StatusMessage = $"{PendingMatches.Count} pending match(es)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApproveAsync(PaymentMatch match)
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Approve Match",
            $"Approve payment of R{match.BankPayment?.Amount:N2} for invoice {match.Invoice?.InvoiceNumber}?",
            "Yes", "No");

        if (!confirmed) return;

        var success = await _apiClient.ApprovePaymentMatchAsync(match.Id);
        if (success)
        {
            await Shell.Current.DisplayAlert("Approved", "Payment approved and recorded", "OK");
            await LoadMatchesAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to approve match", "OK");
        }
    }

    [RelayCommand]
    private async Task RejectAsync(PaymentMatch match)
    {
        var reason = await Shell.Current.DisplayPromptAsync("Reject Match", "Reason for rejection:");
        if (string.IsNullOrWhiteSpace(reason)) return;

        var success = await _apiClient.RejectPaymentMatchAsync(match.Id, reason);
        if (success)
        {
            await Shell.Current.DisplayAlert("Rejected", "Match rejected", "OK");
            await LoadMatchesAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to reject match", "OK");
        }
    }
}
