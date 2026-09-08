using Microsoft.Extensions.DependencyInjection;
using RamsElec.App.Services;
using RamsElec.App.Views;

namespace RamsElec.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        MauiProgram.Log("CreateWindow start");
        try
        {
#if WINDOWS
            var dashboard = MauiProgram.Services?.GetRequiredService<DashboardPage>();
            var window = new Window(new NavigationPage(dashboard));
            MauiProgram.Log("CreateWindow success (Windows NavigationPage)");
            return window;
#else
            var window = new Window(new AppShell());
            MauiProgram.Log("CreateWindow success");
            return window;
#endif
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"CreateWindow error: {ex}");
            throw;
        }
    }
}
