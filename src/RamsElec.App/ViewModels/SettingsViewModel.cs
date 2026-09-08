using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;

namespace RamsElec.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AuthService _authService;

    public SettingsViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    [RelayCommand]
    private void LoadSettings()
    {
        DisplayName = _authService.DisplayName ?? "Unknown";
        Role = _authService.Role ?? "Unknown";
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        Application.Current!.Windows[0].Page = new NavigationPage(
            Application.Current.Windows[0].Page!.Handler!.MauiContext!.Services
                .GetRequiredService<Views.LoginPage>());
    }

    [RelayCommand]
    private async Task GoToCompanySettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.CompanySettingsPage));
    }
}
