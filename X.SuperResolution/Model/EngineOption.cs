// -- Function: EngineOption.cs
// --- Project: X.SuperResolution
// ---- Remark:
// ---- Author: Lucifer
// ------ Date: 2026-01-24 22:01:05

// ReSharper disable InconsistentNaming
namespace X.SuperResolution.Model;

public class EngineOption
{
    /// <summary>
    /// 降噪级别
    /// </summary>
    public Dictionary<int, string> noise_levels { get; set; } = [];

    /// <summary>
    /// 缩放倍率
    /// </summary>
    public Dictionary<int, string> scales { get; set; } = [];

    /// <summary>
    /// 模型算法
    /// </summary>
    public Dictionary<int, string> model_paths { get; set; } = [];

    /// <summary>
    /// 模型名称
    /// </summary>
    public Dictionary<int, string> model_names { get; set; } = [];

    /// <summary>
    /// 输出格式
    /// </summary>
    public Dictionary<int, string> output_formats { get; set; } = [];

    /// <summary>
    /// 分块大小
    /// </summary>
    public Dictionary<int, string> tile_sizes { get; set; } = [];

    /// <summary>
    /// 线程数量
    /// </summary>
    public Dictionary<int, string> thread_counts { get; set; } = [];
}
