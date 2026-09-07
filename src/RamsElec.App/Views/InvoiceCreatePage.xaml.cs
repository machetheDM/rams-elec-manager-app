using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class InvoiceCreatePage : ContentPage
{
    private readonly InvoiceCreateViewModel _viewModel;

    public InvoiceCreatePage(InvoiceCreateViewModel viewModel)
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
