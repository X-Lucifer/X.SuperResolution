using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace X.SuperResolution.Model;
// ReSharper disable InconsistentNaming
public partial class TaskItem : ReactiveObject
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
    [Reactive]
    private string path;

    /// <summary>
    /// 进度 0-100
    /// </summary>
    [Reactive]
    private int progress;

    /// <summary>
    /// 状态描述
    /// </summary>
    [Reactive]
    private string status = "等待中";
}
