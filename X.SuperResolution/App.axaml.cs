using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using X.SuperResolution.ViewModels;
using X.SuperResolution.Views;

namespace X.SuperResolution;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = Locator.Current.GetService<IServiceCollection>();
        var provider = ConfigureServices(services);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var vm = provider.GetRequiredService<MainWindowViewModel>();
        desktop.MainWindow = new MainWindow
        {
            DataContext = vm
        };
        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        return services.BuildServiceProvider();
    }
}