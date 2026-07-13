using CommunityToolkit.Mvvm.ComponentModel;

namespace X.SuperResolution.Model;
// ReSharper disable InconsistentNaming
public partial class TaskItem : ObservableObject
{
    private const string EmptyElapsedText = "00:00:00";

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

    /// <summary>
    /// 当前子任务开始处理时间
    /// </summary>
    [ObservableProperty]
    private DateTime? startedAt;

    /// <summary>
    /// 当前子任务处理完成时间
    /// </summary>
    [ObservableProperty]
    private DateTime? finishedAt;

    /// <summary>
    /// 当前子任务耗时显示文本
    /// </summary>
    [ObservableProperty]
    private string elapsedText = EmptyElapsedText;

    public void ResetTiming()
    {
        StartedAt = null;
        FinishedAt = null;
        ElapsedText = EmptyElapsedText;
    }

    public void StartTiming(DateTime start_time)
    {
        StartedAt = start_time;
        FinishedAt = null;
        ElapsedText = EmptyElapsedText;
    }

    public void StopTiming(DateTime finish_time)
    {
        if (StartedAt == null) return;

        FinishedAt = finish_time;
        UpdateElapsed(finish_time);
    }

    public void UpdateElapsed(DateTime now)
    {
        if (StartedAt == null) return;

        var end_time = FinishedAt ?? now;
        var elapsed = end_time - StartedAt.Value;
        ElapsedText = FormatElapsed(elapsed);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;

        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
}
