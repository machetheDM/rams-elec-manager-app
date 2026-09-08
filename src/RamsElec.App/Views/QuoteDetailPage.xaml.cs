using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class QuoteDetailPage : ContentPage
{
    private readonly QuoteDetailViewModel _viewModel;

    public QuoteDetailPage(QuoteDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadQuoteCommand.ExecuteAsync(null);
    }
}
