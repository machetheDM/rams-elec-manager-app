using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class QuoteCreatePage : ContentPage
{
    private readonly QuoteCreateViewModel _viewModel;

    public QuoteCreatePage(QuoteCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
