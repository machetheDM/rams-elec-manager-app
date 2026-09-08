using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using RamsElec.App.Services;
using RamsElec.App.ViewModels;
using RamsElec.App.Views;

namespace RamsElec.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .UseLiveCharts()
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

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
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

        return builder.Build();
    }
}
