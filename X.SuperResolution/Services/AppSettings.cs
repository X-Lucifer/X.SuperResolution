namespace X.SuperResolution.Services;

public sealed class AppSettings
{
    /// <summary>
    /// 默认语系
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 界面主题（Light / Dark）
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                field = Path.Combine(AppContext.BaseDirectory, "output");
                if (!Directory.Exists(field))
                {
                    Directory.CreateDirectory(field);
                }
            }
            return field;
        }
        set;
    }
}
