using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class QuoteListPage : ContentPage
{
    private readonly QuoteListViewModel _viewModel;

    public QuoteListPage(QuoteListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadQuotesCommand.ExecuteAsync(null);
    }
}
