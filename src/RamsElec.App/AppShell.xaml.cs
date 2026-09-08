using RamsElec.App.Views;

namespace RamsElec.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(InvoiceCreatePage), typeof(InvoiceCreatePage));
        Routing.RegisterRoute(nameof(InvoiceDetailPage), typeof(InvoiceDetailPage));
        Routing.RegisterRoute(nameof(CompanySettingsPage), typeof(CompanySettingsPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }
}
