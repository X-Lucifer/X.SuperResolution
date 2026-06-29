namespace X.SuperResolution.Engine;

public sealed class NcnnProgressChangedEventArgs : EventArgs
{
    public int TaskId { get; init; }
    public NcnnTaskState State { get; init; }
    public int CurrentImage { get; init; }
    public int TotalImages { get; init; }
    public int CompletedImages { get; init; }
    public int Percent { get; init; }
    public string CurrentFile { get; init; }
}
