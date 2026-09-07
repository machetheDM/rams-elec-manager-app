using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class InvoiceListPage : ContentPage
{
    private readonly InvoiceListViewModel _viewModel;

    public InvoiceListPage(InvoiceListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadInvoicesCommand.ExecuteAsync(null);
    }
}
