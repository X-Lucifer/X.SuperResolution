namespace X.SuperResolution.Services;

public sealed class AppSettings
{
    /// <summary>
    /// 默认语系
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    private string _output_directory;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_output_directory))
            {
                _output_directory = Path.Combine(AppContext.BaseDirectory, "output");
                if (!Directory.Exists(_output_directory))
                {
                    Directory.CreateDirectory(_output_directory);
                }
            }
            return _output_directory;
        }
        set => _output_directory = value;
    }
}
