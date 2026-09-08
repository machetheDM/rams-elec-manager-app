using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _viewModel;

    public AnalyticsPage(AnalyticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAnalyticsCommand.ExecuteAsync(null);
    }
}
