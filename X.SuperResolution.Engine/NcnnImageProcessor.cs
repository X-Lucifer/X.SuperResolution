using System.Runtime.InteropServices;
using static X.SuperResolution.Engine.NcnnImageProcNative;

namespace X.SuperResolution.Engine;

public sealed class NcnnImageProcessor : IDisposable
{
    private static readonly object _initialization_lock = new();
    private static int _reference_count;

    private bool _disposed;

    public NcnnImageProcessor()
    {
        EnsureInitialized();
    }

    public static string GetVersion()
    {
        SetDllDirectory(AppContext.BaseDirectory);
        var pointer = NcnnImageProc_GetVersion();
        return pointer == IntPtr.Zero ? "unknown" : Marshal.PtrToStringUTF8(pointer) ?? "unknown";
    }

    public NcnnImageProcessingTask CreateTask(NcnnTaskConfig config)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var native_config = new NativeTaskConfigMemory(config);
        var task_id = NcnnImageProc_CreateTask(ref native_config.Value);
        if (task_id < 0)
        {
            native_config.Dispose();
            throw new InvalidOperationException($"NcnnImageProc_CreateTask failed with code {task_id}");
        }

        return new NcnnImageProcessingTask(task_id, native_config);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_initialization_lock)
        {
            if (_reference_count <= 0) return;

            _reference_count--;
            if (_reference_count == 0)
                NcnnImageProc_Deinit();
        }
    }

    private static void EnsureInitialized()
    {
        lock (_initialization_lock)
        {
            if (_reference_count == 0)
            {
                SetDllDirectory(AppContext.BaseDirectory);
                var result = NcnnImageProc_Init();
                if (result != 0)
                    throw new InvalidOperationException($"NcnnImageProc_Init failed with code {result}");
            }

            _reference_count++;
        }
    }
}

public sealed class NcnnImageProcessingTask : IDisposable, IAsyncDisposable
{
    private static readonly NcnnProgressCallback _progress_callback = OnNativeProgress;

    private readonly NativeTaskConfigMemory _config_memory;
    private readonly TaskCompletionSource<NcnnTaskState> _completion_source = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private GCHandle _self_handle;

    private int _disposed;
    private NcnnTaskState _last_state = NcnnTaskState.Idle;

    internal NcnnImageProcessingTask(int task_id, NativeTaskConfigMemory config_memory)
    {
        TaskId = task_id;
        _config_memory = config_memory;
        _self_handle = GCHandle.Alloc(this);
        NcnnImageProc_SetCallback(TaskId, _progress_callback, GCHandle.ToIntPtr(_self_handle));
    }

    public int TaskId { get; }

    public event EventHandler<NcnnProgressChangedEventArgs> ProgressChanged;

    public void Start()
    {
        ThrowIfDisposed();

        var result = NcnnImageProc_StartTask(TaskId);
        if (result != 0)
            throw new InvalidOperationException($"NcnnImageProc_StartTask failed with code {result}");
    }

    public async Task<NcnnTaskState> StartAndWaitAsync(CancellationToken cancellation_token)
    {
        Start();

        var cancellation_requested = false;
        await using var registration = cancellation_token.CanBeCanceled
            ? cancellation_token.Register(static state =>
            {
                var task = (NcnnImageProcessingTask)state!;
                task.Cancel();
            }, this)
            : default;

        while (true)
        {
            if (cancellation_token.IsCancellationRequested)
                cancellation_requested = true;

            if (_completion_source.Task.IsCompleted)
            {
                var state = await _completion_source.Task.ConfigureAwait(false);
                if (cancellation_requested && state == NcnnTaskState.Cancelled)
                    throw new OperationCanceledException(cancellation_token);

                return state;
            }

            var current_state = GetStatus();
            if (IsTerminalState(current_state))
                Complete(current_state);

            await Task.Delay(100, cancellation_token).ConfigureAwait(false);
        }
    }

    public void Pause()
    {
        ThrowIfDisposed();

        var result = NcnnImageProc_PauseTask(TaskId);
        if (result != 0)
            throw new InvalidOperationException($"NcnnImageProc_PauseTask failed with code {result}");
    }

    public void Resume()
    {
        ThrowIfDisposed();

        var result = NcnnImageProc_ResumeTask(TaskId);
        if (result != 0)
            throw new InvalidOperationException($"NcnnImageProc_ResumeTask failed with code {result}");
    }

    public void Cancel()
    {
        if (Volatile.Read(ref _disposed) != 0) return;
        NcnnImageProc_CancelTask(TaskId);
    }

    private NcnnTaskState GetStatus()
    {
        ThrowIfDisposed();
        return NcnnImageProc_GetStatus(TaskId);
    }

    public int GetProgress()
    {
        ThrowIfDisposed();
        return NcnnImageProc_GetProgress(TaskId);
    }

    public int GetCompletedCount()
    {
        ThrowIfDisposed();
        return NcnnImageProc_GetCompletedCount(TaskId);
    }

    public int GetTotalCount()
    {
        ThrowIfDisposed();
        return NcnnImageProc_GetTotalCount(TaskId);
    }

    private string GetLastError()
    {
        ThrowIfDisposed();

        var pointer = NcnnImageProc_GetLastError(TaskId);
        return pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        try
        {
            NcnnImageProc_SetCallback(TaskId, null, IntPtr.Zero);

            if (!IsTerminalState(_last_state))
                NcnnImageProc_CancelTask(TaskId);

            // The native wrapper sends the terminal callback before its runner thread
            // has fully returned, so destruction is intentionally deferred a little.
            await Task.Delay(1000).ConfigureAwait(false);
            NcnnImageProc_DestroyTask(TaskId);
        }
        finally
        {
            if (_self_handle.IsAllocated)
                _self_handle.Free();

            _config_memory.Dispose();
        }
    }

    private void Complete(NcnnTaskState state)
    {
        _last_state = state;

        if (state == NcnnTaskState.Failed)
            _completion_source.TrySetException(new InvalidOperationException(GetLastError()));
        else
            _completion_source.TrySetResult(state);
    }

    private void OnProgress(NcnnProgressInfoNative info)
    {
        _last_state = info.state;

        var args = new NcnnProgressChangedEventArgs
        {
            TaskId = info.taskId,
            State = info.state,
            CurrentImage = info.currentImage,
            TotalImages = info.totalImages,
            CompletedImages = info.completedImages,
            Percent = Math.Clamp(info.percent, 0, 100),
            CurrentFile = info.currentFile == IntPtr.Zero ? null : Marshal.PtrToStringUni(info.currentFile)
        };

        try
        {
            ProgressChanged?.Invoke(this, args);
        }
        catch
        {
            // Do not let managed UI/event exceptions escape into unmanaged code.
        }

        if (IsTerminalState(info.state))
            Complete(info.state);
    }

    private static void OnNativeProgress(ref NcnnProgressInfoNative info, IntPtr user_data)
    {
        if (user_data == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(user_data);
        if (handle.Target is NcnnImageProcessingTask task)
            task.OnProgress(info);
    }

    private static bool IsTerminalState(NcnnTaskState state)
    {
        return state is NcnnTaskState.Completed or NcnnTaskState.Cancelled or NcnnTaskState.Failed;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    }
}

internal sealed class NativeTaskConfigMemory : IDisposable
{
    private readonly List<IntPtr> _allocated_strings = [];

    public NativeTaskConfigMemory(NcnnTaskConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        Value = new NcnnTaskConfigNative
        {
            engineType = config.EngineType,
            inputPath = AllocateString(config.InputPath),
            outputPath = AllocateString(config.OutputPath),
            outputFormat = AllocateString(config.OutputFormat),
            modelPath = AllocateString(config.ModelPath),
            modelName = AllocateString(config.ModelName),
            scale = config.Scale,
            noise = config.Noise,
            gpuId = config.GpuId,
            tilesize = config.TileSize,
            jobsLoad = Math.Max(1, config.JobsLoad),
            jobsProc = Math.Max(1, config.JobsProc),
            jobsSave = Math.Max(1, config.JobsSave),
            ttaMode = config.TtaMode,
            verbose = config.Verbose,
            reserved = new int[8]
        };
    }

    public NcnnTaskConfigNative Value;

    public void Dispose()
    {
        foreach (var pointer in _allocated_strings)
            Marshal.FreeHGlobal(pointer);

        _allocated_strings.Clear();
    }

    private IntPtr AllocateString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return IntPtr.Zero;

        var pointer = Marshal.StringToHGlobalUni(value);
        _allocated_strings.Add(pointer);
        return pointer;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NcnnTaskConfigNative
{
    public NcnnEngineType engineType;
    public IntPtr inputPath;
    public IntPtr outputPath;
    public IntPtr outputFormat;
    public IntPtr modelPath;
    public IntPtr modelName;
    public int scale;
    public int noise;
    public int gpuId;
    public int tilesize;
    public int jobsLoad;
    public int jobsProc;
    public int jobsSave;
    public int ttaMode;
    public int verbose;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public int[] reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NcnnProgressInfoNative
{
    public int taskId;
    public NcnnTaskState state;
    public int currentImage;
    public int totalImages;
    public int completedImages;
    public int percent;
    public IntPtr currentFile;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NcnnProgressCallback(ref NcnnProgressInfoNative info, IntPtr user_data);

internal static partial class NcnnImageProcNative
{
    private const string DllName = "lucifer_ncnn_vulkan";
    [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectoryW(string path_name);

    public static void SetDllDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return;

        if (!SetDllDirectoryW(directory))
            throw new InvalidOperationException($"SetDllDirectoryW failed with Win32 error {Marshal.GetLastWin32Error()}");
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_Init();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void NcnnImageProc_Deinit();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_CreateTask(ref NcnnTaskConfigNative config);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void NcnnImageProc_SetCallback(int task_id, NcnnProgressCallback callback, IntPtr user_data);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_StartTask(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_PauseTask(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_ResumeTask(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_CancelTask(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern NcnnTaskState NcnnImageProc_GetStatus(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_GetProgress(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_GetCompletedCount(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int NcnnImageProc_GetTotalCount(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr NcnnImageProc_GetLastError(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void NcnnImageProc_DestroyTask(int task_id);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr NcnnImageProc_GetVersion();
}
