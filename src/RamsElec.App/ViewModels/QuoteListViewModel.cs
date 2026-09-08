using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.ViewModels;

public partial class QuoteListViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public QuoteListViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ObservableCollection<QuoteDto> Quotes { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadQuotesAsync()
    {
        IsLoading = true;
        try
        {
            var quotes = await _apiClient.GetQuotesAsync();
            Quotes.Clear();
            foreach (var q in quotes)
                Quotes.Add(q);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToCreateAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.QuoteCreatePage));
    }

    [RelayCommand]
    private async Task OpenQuoteAsync(QuoteDto quote)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.QuoteDetailPage)}?QuoteId={quote.Id}");
    }
}
