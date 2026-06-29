namespace X.SuperResolution.Engine;

/// <summary>
/// Native task lifecycle state exported by lucifer_ncnn_vulkan.dll.
/// </summary>
public enum NcnnTaskState
{
    Idle = 0,
    Loading = 1,
    Processing = 2,
    Saving = 3,
    Completed = 4,
    Cancelled = 5,
    Paused = 6,
    Failed = 7
}
