using CommunityToolkit.Mvvm.ComponentModel;

namespace X.SuperResolution.Model;
// ReSharper disable InconsistentNaming
public partial class TaskItem : ObservableObject
{
    /// <summary>
    /// 文件id
    /// </summary>
    public string Oid { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [ObservableProperty]
    private string path;

    /// <summary>
    /// 进度 0-100
    /// </summary>
    [ObservableProperty]
    private int progress;

    /// <summary>
    /// 状态描述
    /// </summary>
    [ObservableProperty]
    private string status = "等待中";
}
