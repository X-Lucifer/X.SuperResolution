using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace X.SuperResolution.Services;

public static class LocalizationService
{
    public static void ApplyLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language) || Application.Current is not { } app)
        {
            return;
        }

        var lang_file = $"avares://{typeof(Program).Assembly.GetName().Name}/Assets/Languages/{language}.axaml";
        var uri = new Uri(lang_file, UriKind.Absolute);
        app.Resources.MergedDictionaries[0] = new ResourceInclude(uri)
        {
            Source = uri
        };
    }
}