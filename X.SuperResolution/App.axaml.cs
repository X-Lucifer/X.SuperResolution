using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using X.SuperResolution.Services;
using X.SuperResolution.ViewModels;
using X.SuperResolution.Views;
using static X.SuperResolution.Services.LocalizationService;

namespace X.SuperResolution;

public partial class App : Application
{
    private ServiceProvider _service_provider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _service_provider = ConfigureServices();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var vm = _service_provider.GetRequiredService<MainWindowViewModel>();
        ApplyLanguage(vm.CurrentLang);
        ApplyTheme(vm.CurrentTheme);
        desktop.MainWindow = new MainWindow
        {
            DataContext = vm
        };
        base.OnFrameworkInitializationCompleted();
    }

    public static void ApplyTheme(string theme)
    {
        if (Current is not App app)
        {
            return;
        }

        app.RequestedThemeVariant = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase)
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SettingsService>();
        services.AddSingleton(x => new MainWindowViewModel(x.GetRequiredService<SettingsService>()));
        return services.BuildServiceProvider();
    }
}