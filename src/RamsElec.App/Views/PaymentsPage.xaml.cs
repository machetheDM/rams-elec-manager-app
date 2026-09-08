using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly PaymentsViewModel _viewModel;

    public PaymentsPage(PaymentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMatchesCommand.ExecuteAsync(null);
    }
}
