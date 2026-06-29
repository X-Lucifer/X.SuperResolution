namespace X.SuperResolution.Engine;

/// <summary>
/// Managed configuration for a lucifer_ncnn_vulkan.dll task.
/// </summary>
public sealed class NcnnTaskConfig
{
    public NcnnEngineType EngineType { get; init; }
    public string InputPath { get; init; }
    public string OutputPath { get; init; }
    public string OutputFormat { get; init; } = "png";
    public string ModelPath { get; init; }
    public string ModelName { get; init; }
    public int Scale { get; init; }
    public int Noise { get; init; }
    public int GpuId { get; init; } = 9999;
    public int TileSize { get; init; }
    public int JobsLoad { get; init; } = 1;
    public int JobsProc { get; init; } = 2;
    public int JobsSave { get; init; } = 2;
    public int TtaMode { get; init; }
    public int Verbose { get; init; }
}