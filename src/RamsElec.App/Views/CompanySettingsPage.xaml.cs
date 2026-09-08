using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class CompanySettingsPage : ContentPage
{
    private readonly CompanySettingsViewModel _viewModel;

    public CompanySettingsPage(CompanySettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCompanyCommand.ExecuteAsync(null);
    }
}
