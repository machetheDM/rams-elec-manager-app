using System.IO;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RamsElec.App.Services;
using RamsElec.App.ViewModels;
using RamsElec.App.Views;

namespace RamsElec.App;

public static class MauiProgram
{
    private static readonly string LogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RamsElec");
    private static readonly string LogFile = Path.Combine(LogDir, "startup.log");

    internal static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}");
        }
        catch { }
    }

    public static IServiceProvider? Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        Log("CreateMauiApp start");
        AppDomain.CurrentDomain.UnhandledException += (s, e) => Log($"Unhandled: {e.ExceptionObject}");
        TaskScheduler.UnobservedTaskException += (s, e) => Log($"Unobserved: {e.Exception}");

        try
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Services
            builder.Services.AddSingleton<LocalDatabase>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<SyncService>();
            builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();
            builder.Services.AddTransient<InvoiceListViewModel>();
            builder.Services.AddTransient<InvoiceCreateViewModel>();
            builder.Services.AddTransient<InvoiceDetailViewModel>();
            builder.Services.AddTransient<QuoteListViewModel>();
            builder.Services.AddTransient<QuoteCreateViewModel>();
            builder.Services.AddTransient<QuoteDetailViewModel>();
            builder.Services.AddTransient<CompanySettingsViewModel>();
            builder.Services.AddTransient<JobsViewModel>();
            builder.Services.AddTransient<PaymentsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<AnalyticsPage>();
            builder.Services.AddTransient<InvoiceListPage>();
            builder.Services.AddTransient<InvoiceCreatePage>();
            builder.Services.AddTransient<InvoiceDetailPage>();
            builder.Services.AddTransient<QuoteListPage>();
            builder.Services.AddTransient<QuoteCreatePage>();
            builder.Services.AddTransient<QuoteDetailPage>();
            builder.Services.AddTransient<CompanySettingsPage>();
            builder.Services.AddTransient<JobsPage>();
            builder.Services.AddTransient<PaymentsPage>();
            builder.Services.AddTransient<SettingsPage>();

            var app = builder.Build();
            Services = app.Services;
            Log("CreateMauiApp success");
            return app;
        }
        catch (Exception ex)
        {
            Log($"CreateMauiApp error: {ex}");
            throw;
        }
    }
}
