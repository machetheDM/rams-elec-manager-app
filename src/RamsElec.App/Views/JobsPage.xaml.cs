using RamsElec.App.ViewModels;

namespace RamsElec.App.Views;

public partial class JobsPage : ContentPage
{
    private readonly JobsViewModel _viewModel;

    public JobsPage(JobsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadJobsCommand.ExecuteAsync(null);
    }
}
