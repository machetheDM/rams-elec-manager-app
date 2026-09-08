using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class InvoiceDetailPage : ContentPage
{
    private readonly InvoiceDetailViewModel _viewModel;

    public InvoiceDetailPage(InvoiceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadInvoiceCommand.ExecuteAsync(null);
    }
}
