using System.Text.Json;

namespace X.SuperResolution.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions _json_options = new()
    {
        WriteIndented = true
    };

    private readonly string _settings_path;

    public SettingsService()
    {
        _settings_path = AppContext.BaseDirectory;
        Current = Load();
    }

    public AppSettings Current { get; private set; }

    private void Save()
    {
        var settings_dir = Path.GetDirectoryName(_settings_path);
        if (!string.IsNullOrWhiteSpace(settings_dir))
        {
            Directory.CreateDirectory(settings_dir);
        }

        var json = JsonSerializer.Serialize(Current, _json_options);
        var file = Path.Combine(_settings_path, "settings.json");
        File.WriteAllText(file, json);
    }

    public void SetLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language) || Current.Language == language)
        {
            return;
        }

        Current.Language = language;
        Save();
    }

    public void SetTheme(string theme)
    {
        var normalized_theme = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase)
            ? "Dark"
            : "Light";
        if (Current.Theme == normalized_theme)
        {
            return;
        }

        Current.Theme = normalized_theme;
        Save();
    }

    public void SetOutputDirectory(string output_directory)
    {
        if (string.IsNullOrWhiteSpace(output_directory) || Current.OutputDirectory == output_directory)
        {
            return;
        }

        Current.OutputDirectory = output_directory;
        Save();
    }

    private AppSettings Load()
    {
        var file = Path.Combine(_settings_path, "settings.json");
        if (!File.Exists(file))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(file);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _json_options);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }
}