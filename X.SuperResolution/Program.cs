using Avalonia;
using AppBuilder = Avalonia.AppBuilder;

namespace X.SuperResolution;

internal static class Program
{
    private const string AppMutexName = "X.Lucifer.SuperResolution";

    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, AppMutexName, out var created_new);
        if (!created_new)
        {
            return;
        }

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        GC.KeepAlive(mutex);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToTrace()
            ;
    }
}
