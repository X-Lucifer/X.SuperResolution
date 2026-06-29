using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Collections.Frozen;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using X.SuperResolution.Engine;
using X.SuperResolution.Model;
using TaskItem = X.SuperResolution.Model.TaskItem;

// ReSharper disable InconsistentNaming
namespace X.SuperResolution.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private TaskItem _current_processing_task;
    private NcnnImageProcessor _processor;
    private NcnnImageProcessingTask _current_native_task;
    private readonly StringBuilder _log_builder = new();
    private long _last_progress_tick;
    private NcnnTaskState? _last_native_state;

    private CancellationTokenSource _current_task_cts;
    private bool _stop_requested;

    private sealed class ProcessingSettings
    {
        public EngineType Engine { get; init; }
        public string ModelPath { get; init; }
        public string ModelName { get; init; }
        public string NoiseLevel { get; init; }
        public string Scale { get; init; }
        public string ThreadCount { get; init; }
        public string TileSize { get; init; }
        public string OutputFormat { get; init; }
        public string GpuId { get; init; }
        public int TtaMode { get; init; }
    }

    public MainWindowViewModel()
    {
    }

    /// <inheritdoc />
    public MainWindowViewModel(ISukiDialogManager dialog_manager, ISukiToastManager toast_manager)
    {
        _dialogManager = dialog_manager;
        _toastManager = toast_manager;
    }

    /// <summary>
    /// 引擎选项
    /// </summary>
    private static readonly FrozenDictionary<EngineType, EngineOption> _engine_options =
        new Dictionary<EngineType, EngineOption>
        {
            {
                EngineType.waifu2x, new EngineOption
                {
                    noise_levels = new Dictionary<int, string>
                    {
                        { 0, "0" }, { 1, "1" }, { 2, "2" }, { 3, "3" }
                    },
                    scales = new Dictionary<int, string>
                    {
                        { 0, "1" }, { 1, "2" }, { 2, "4" }, { 3, "8" }, { 4, "16" }, { 5, "32" }
                    },
                    model_paths = new Dictionary<int, string>
                    {
                        { 0, "models-cunet" },
                        { 1, "models-upconv_7_photo" },
                        { 2, "models-upconv_7_anime_style_art_rgb" }
                    },
                    output_formats = new Dictionary<int, string>
                    {
                        { 0, "png" }, { 1, "jpg" }, { 2, "webp" }
                    },
                    tile_sizes = new Dictionary<int, string>
                    {
                        { 0, "0" }, { 1, "32" }, { 2, "64" }, { 3, "128" }, { 4, "256" }, { 5, "512" }
                    },
                    thread_counts = new Dictionary<int, string>
                    {
                        { 0, "1:2:2" }, { 1, "2:2:2" }, { 2, "2:4:2" }, { 3, "4:4:4" }
                    }
                }
            },
            {
                EngineType.RealESRGAN, new EngineOption
                {
                    noise_levels = new Dictionary<int, string>
                    {
                        { 0, "0" }
                    },
                    scales = new Dictionary<int, string>
                    {
                        { 0, "2" }, { 1, "3" }, { 2, "4" }
                    },
                    model_paths = new Dictionary<int, string>
                    {
                        { 0, "models" }
                    },
                    model_names = new Dictionary<int, string>
                    {
                        { 0, "realesr-animevideov3" },
                        { 1, "realesrgan-x4plus" },
                        { 2, "realesrgan-x4plus-anime" },
                        { 3, "realesrnet-x4plus" }
                    },
                    output_formats = new Dictionary<int, string>
                    {
                        { 0, "png" }, { 1, "jpg" }, { 2, "webp" }
                    },
                    tile_sizes = new Dictionary<int, string>
                    {
                        { 0, "0" }, { 1, "32" }, { 2, "64" }, { 3, "128" }, { 4, "256" }, { 5, "512" }
                    },
                    thread_counts = new Dictionary<int, string>
                    {
                        { 0, "1:2:2" }, { 1, "2:2:2" }, { 2, "2:4:2" }, { 3, "4:4:4" }
                    }
                }
            },
            {
                EngineType.srmd, new EngineOption
                {
                    noise_levels = new Dictionary<int, string>
                    {
                        { 0, "0" }, { 1, "1" }, { 2, "2" }, { 3, "3" }, { 4, "4" },
                        { 5, "5" }, { 6, "6" }, { 7, "7" }, { 8, "8" }, { 9, "9" }, { 10, "10" }
                    },
                    scales = new Dictionary<int, string>
                    {
                        { 0, "2" }, { 1, "3" }, { 2, "4" }
                    },
                    model_paths = new Dictionary<int, string>
                    {
                        { 0, "models-srmd" }
                    },
                    output_formats = new Dictionary<int, string>
                    {
                        { 0, "png" }, { 1, "jpg" }, { 2, "webp" }
                    },
                    tile_sizes = new Dictionary<int, string>
                    {
                        { 0, "0" }, { 1, "32" }, { 2, "64" }, { 3, "128" }, { 4, "256" }, { 5, "512" }
                    },
                    thread_counts = new Dictionary<int, string>
                    {
                        { 0, "1:2:2" }, { 1, "2:2:2" }, { 2, "2:4:2" }, { 3, "4:4:4" }
                    }
                }
            }
        }.ToFrozenDictionary();

    [Reactive(
        nameof(IsWaifu2x),
        nameof(IsSrmd),
        nameof(IsRealESRGAN),
        nameof(HasModelNameOption),
        nameof(CurrentEngineOption),
        nameof(CurrentSelectedModelNameIndex),
        nameof(CurrentSelectedModelPathIndex),
        nameof(CurrentSelectedNoiseLevelIndex),
        nameof(CurrentSelectedOutputFormatIndex),
        nameof(CurrentSelectedTileSizeIndex),
        nameof(CurrentSelectedThreadCountIndex)
    )]
    private EngineType currentEngine = EngineType.waifu2x;

    public bool IsWaifu2x => CurrentEngine == EngineType.waifu2x;

    public bool IsSrmd => CurrentEngine == EngineType.srmd;

    public bool IsRealESRGAN => CurrentEngine == EngineType.RealESRGAN;

    public bool HasModelNameOption => CurrentEngineOption.model_names.Count > 0;

    [Reactive] private int currentSelectedNoiseLevelIndex;

    [Reactive] private int currentSelectedScaleIndex;

    [Reactive] private int currentSelectedModelPathIndex;

    [Reactive] private int currentSelectedModelNameIndex;

    [Reactive] private int currentSelectedOutputFormatIndex;

    [Reactive] private int currentSelectedTileSizeIndex;

    [Reactive] private int currentSelectedThreadCountIndex;

    [Reactive] private int selectedGpuIndex;

    [Reactive] private bool ttaMode;

    public static string CurrentLang => "zh-CN";

    public EngineOption CurrentEngineOption => _engine_options[CurrentEngine];

    [Reactive] private ISukiDialogManager _dialogManager;

    [Reactive] private ISukiToastManager _toastManager;

    [Reactive] private Dictionary<string, string> langList = new()
    {
        { "zh-CN", "中文" },
        { "en-US", "English" }
    };

    [Reactive] private ObservableCollection<TaskItem> taskList = [];

    /// <summary>
    /// 日志文本
    /// </summary>
    [Reactive] private string logText = string.Empty;

    /// <summary>
    /// 是否正在处理任务
    /// </summary>
    [Reactive] private bool isProcessing;

    public bool CanStartTask
    {
        get { return !IsProcessing && TaskList.Any(task => !IsTaskCompleted(task)); }
    }

    public bool CanCancelTask => IsProcessing && _current_task_cts != null;

    public bool CanClearTask => !IsProcessing && TaskList.Count > 0;

    /// <summary>
    /// 切换引擎
    /// </summary>
    [ReactiveCommand]
    private void ChangeEngine(EngineType type)
    {
        CurrentEngine = type;
        CurrentSelectedNoiseLevelIndex = 0;
        CurrentSelectedScaleIndex = type == EngineType.RealESRGAN ? 2 : 0;
        CurrentSelectedModelPathIndex = 0;
        CurrentSelectedModelNameIndex = 0;
        CurrentSelectedOutputFormatIndex = 0;
        CurrentSelectedTileSizeIndex = 0;
        CurrentSelectedThreadCountIndex = 0;
    }

    /// <summary>
    /// 选择文件
    /// </summary>
    [ReactiveCommand]
    private async Task SelectFile(CancellationToken token)
    {
        try
        {
            var files = await OpenFilePickerAsync();
            if (files is not { Count: > 0 }) return;

            const long maxFileSize = 50L * 1024 * 1024;
            var added_count = 0;

            foreach (var item in files)
            {
                var path = item.Path.IsFile ? item.Path.LocalPath : item.Path.AbsolutePath;
                var file_info = new FileInfo(path);
                if (file_info.Length > maxFileSize)
                {
                    AppendLog($"[提示] 跳过超过 50MB 的文件: {item.Name}");
                    continue;
                }

                var task_item = new TaskItem
                {
                    Oid = Guid.NewGuid().ToString("D"),
                    Name = item.Name,
                    Path = path,
                    FileSize = file_info.Length
                };
                TaskList.Add(task_item);
                added_count++;
            }

            // 按文件大小升序排列，小文件优先
            var sorted = TaskList.OrderBy(t => t.FileSize).ToList();
            TaskList.Clear();
            foreach (var t in sorted) TaskList.Add(t);

            if (added_count > 0)
                AppendLog($"已添加 {added_count} 个文件");

            NotifyTaskControlStateChanged();
        }
        catch (Exception ex)
        {
            AppendLog($"[错误] 选择文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 开始处理任务
    /// </summary>
    [ReactiveCommand]
    private async Task StartTask(CancellationToken token)
    {
        if (!CanStartTask) return;

        var settings = CreateProcessingSettings();
        if (!ValidateProcessingSettings(settings)) return;

        var pending_tasks = TaskList
            .Where(task => !IsTaskCompleted(task))
            .OrderBy(task => task.FileSize)
            .ToList();
        if (pending_tasks.Count == 0)
        {
            AppendLog("没有需要处理的任务");
            NotifyTaskControlStateChanged();
            return;
        }

        IsProcessing = true;
        _stop_requested = false;
        _current_task_cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        NotifyTaskControlStateChanged();

        AppendLog("----- 开始处理 -----");

        try
        {
            EnsureProcessor();
            AppendLog($"引擎已初始化: lucifer_ncnn_vulkan {NcnnImageProcessor.GetVersion()}");

            foreach (var task in pending_tasks.Where(task => TaskList.Contains(task)))
            {
                if (_current_task_cts.IsCancellationRequested) break;

                task.Status = "处理中...";
                task.Progress = 0;
                _current_processing_task = task;
                _last_native_state = null;
                NotifyTaskControlStateChanged();

                AppendLog($"开始处理: {task.Name}");

                try
                {
                    var result = await ProcessCurrentTaskAsync(task, settings, _current_task_cts.Token);

                    if (result == NcnnTaskState.Completed)
                    {
                        task.Progress = 100;
                        task.Status = "完成";
                        AppendLog($"完成: {task.Name}");
                    }
                    else if (result == NcnnTaskState.Cancelled)
                    {
                        task.Status = _stop_requested ? "已停止" : "已取消";
                        AppendLog(_stop_requested ? $"已停止: {task.Name}" : $"已取消: {task.Name}");
                        break;
                    }
                    else
                    {
                        task.Status = "失败";
                        AppendLog($"[错误] 处理失败: {task.Name} (state={result})");
                    }
                }
                catch (OperationCanceledException)
                {
                    _current_native_task?.Cancel();
                    task.Status = _stop_requested ? "已停止" : "已取消";
                    AppendLog(_stop_requested ? $"已停止: {task.Name}" : $"已取消: {task.Name}");
                    break;
                }
                catch (Exception ex)
                {
                    task.Status = "失败";
                    AppendLog($"[错误] 处理失败: {task.Name} ({ex.Message})");
                }
                finally
                {
                    _current_processing_task = null;
                    _current_native_task = null;
                    NotifyTaskControlStateChanged();
                }
            }
        }
        catch (Exception ex)
        {
            AppendLog($"[错误] {ex.Message}");
        }
        finally
        {
            _current_processing_task = null;
            _current_native_task = null;
            _current_task_cts?.Dispose();
            _current_task_cts = null;

            if (_stop_requested) MarkUnfinishedTasksStopped();
            IsProcessing = false;
            NotifyTaskControlStateChanged();
            AppendLog("----- 处理结束 -----");
        }
    }

    /// <summary>
    /// 取消当前处理队列
    /// </summary>
    [ReactiveCommand]
    private void CancelTask()
    {
        if (!CanCancelTask) return;

        try
        {
            _stop_requested = true;
            MarkUnfinishedTasksStopped(_current_processing_task);
            _current_task_cts?.Cancel();
            _current_native_task?.Cancel();
            if (_current_processing_task != null) _current_processing_task.Status = "正在停止";
            AppendLog("已请求停止当前队列");
        }
        catch (Exception ex)
        {
            AppendLog($"[错误] 停止失败: {ex.Message}");
        }
        finally
        {
            NotifyTaskControlStateChanged();
        }
    }

    /// <summary>
    /// 清空任务列表
    /// </summary>
    [ReactiveCommand]
    private void ClearTask()
    {
        if (!CanClearTask) return;

        TaskList.Clear();
        AppendLog("已清空任务列表");
        NotifyTaskControlStateChanged();
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    [ReactiveCommand]
    private void ClearLog()
    {
        _log_builder.Clear();
        LogText = string.Empty;
    }


    /// <summary>
    /// 移除单个任务
    /// </summary>
    [ReactiveCommand]
    private void RemoveTask(TaskItem task)
    {
        if (_current_processing_task == task)
        {
            AppendLog($"[提示] 无法移除正在执行的任务: {task.Name}");
            return;
        }

        TaskList.Remove(task);
        AppendLog($"已移除: {task.Name}");
        NotifyTaskControlStateChanged();
    }

    private async Task<NcnnTaskState> ProcessCurrentTaskAsync(
        TaskItem task,
        ProcessingSettings settings,
        CancellationToken cancellation_token)
    {
        var input_path = task.Path;
        var output_dir = Path.Combine(AppContext.BaseDirectory, "output");
        Directory.CreateDirectory(output_dir);

        var file_name = Path.GetFileNameWithoutExtension(input_path);
        var extension = settings.OutputFormat;
        var output_path = Path.Combine(output_dir, $"{file_name}_out.{extension}");

        // 如果输出文件已存在则先删除
        if (File.Exists(output_path))
            File.Delete(output_path);

        var config = CreateNativeTaskConfig(input_path, output_path, settings);
        var native_task = EnsureProcessor().CreateTask(config);

        try
        {
            _current_native_task = native_task;
            native_task.ProgressChanged += OnNativeProgress;
            NotifyTaskControlStateChanged();
            return await native_task.StartAndWaitAsync(cancellation_token);
        }
        finally
        {
            native_task.ProgressChanged -= OnNativeProgress;
            await native_task.DisposeAsync();
        }
    }

    private ProcessingSettings CreateProcessingSettings()
    {
        var option = CurrentEngineOption;
        var model_path = GetCurrentValue(option.model_paths, CurrentSelectedModelPathIndex);
        var gpu_id = SelectedGpuIndex >= 0 ? SelectedGpuIndex.ToString() : "0";

        return new ProcessingSettings
        {
            Engine = CurrentEngine,
            ModelPath = ResolveModelPath(model_path),
            ModelName = GetCurrentValue(option.model_names, CurrentSelectedModelNameIndex),
            NoiseLevel = GetCurrentValue(option.noise_levels, CurrentSelectedNoiseLevelIndex),
            Scale = GetCurrentValue(option.scales, CurrentSelectedScaleIndex),
            ThreadCount = GetCurrentValue(option.thread_counts, CurrentSelectedThreadCountIndex, "1:2:2"),
            TileSize = GetCurrentValue(option.tile_sizes, CurrentSelectedTileSizeIndex, "0"),
            OutputFormat = GetCurrentValue(option.output_formats, CurrentSelectedOutputFormatIndex, "png"),
            GpuId = gpu_id,
            TtaMode = TtaMode ? 1 : 0
        };
    }

    private bool ValidateProcessingSettings(ProcessingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ModelPath) || !Directory.Exists(settings.ModelPath))
        {
            AppendLog($"[错误] 模型目录不存在: {settings.ModelPath}");
            return false;
        }

        if (settings.Engine == EngineType.RealESRGAN)
        {
            var (param_path, bin_path) = GetRealESRGANModelFiles(settings);
            if (!File.Exists(param_path) || !File.Exists(bin_path))
            {
                AppendLog($"[错误] Real-ESRGAN 模型文件不存在: {Path.GetFileName(param_path)} / {Path.GetFileName(bin_path)}");
                AppendLog($"[错误] 模型目录: {settings.ModelPath}");
                return false;
            }
        }
        else if (!Directory.EnumerateFiles(settings.ModelPath, "*.param").Any())
        {
            AppendLog($"[错误] 模型目录中没有 .param 文件: {settings.ModelPath}");
            return false;
        }

        return true;
    }

    private static (string ParamPath, string BinPath) GetRealESRGANModelFiles(ProcessingSettings settings)
    {
        var model_name = string.IsNullOrWhiteSpace(settings.ModelName)
            ? "realesr-animevideov3"
            : settings.ModelName;
        var scale = ParseInt(settings.Scale, 4);
        var file_stem = model_name == "realesr-animevideov3"
            ? $"{model_name}-x{scale}"
            : model_name;

        return (
            Path.Combine(settings.ModelPath, $"{file_stem}.param"),
            Path.Combine(settings.ModelPath, $"{file_stem}.bin"));
    }

    private static string ResolveModelPath(string model_path)
    {
        return Path.IsPathRooted(model_path)
            ? model_path
            : Path.Combine(AppContext.BaseDirectory, model_path!);
    }

    private NcnnTaskConfig CreateNativeTaskConfig(string input_path, string output_path, ProcessingSettings settings)
    {
        var (jobs_load, jobs_proc, jobs_save) = ParseThreadCount(settings.ThreadCount);

        return new NcnnTaskConfig
        {
            EngineType = ToNativeEngine(settings.Engine),
            InputPath = input_path,
            OutputPath = output_path,
            OutputFormat = settings.OutputFormat,
            ModelPath = settings.ModelPath,
            ModelName = settings.ModelName,
            Scale = ParseInt(settings.Scale, 2),
            Noise = ParseInt(settings.NoiseLevel, 0),
            GpuId = ParseInt(settings.GpuId, 9999),
            TileSize = ParseInt(settings.TileSize, 0),
            JobsLoad = jobs_load,
            JobsProc = jobs_proc,
            JobsSave = jobs_save,
            TtaMode = settings.TtaMode,
            Verbose = 0
        };
    }

    private NcnnImageProcessor EnsureProcessor()
    {
        return _processor ??= new NcnnImageProcessor();
    }

    private void OnNativeProgress(object sender, NcnnProgressChangedEventArgs e)
    {
        var now = Environment.TickCount64;
        var state_changed = _last_native_state != e.State;
        if (!state_changed && e.State is not (NcnnTaskState.Completed or NcnnTaskState.Cancelled or NcnnTaskState.Failed
                               )
                           && now - Interlocked.Read(ref _last_progress_tick) < 50)
            return;

        Interlocked.Exchange(ref _last_progress_tick, now);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var current_task = _current_processing_task;
            if (current_task == null) return;

            current_task.Progress = CalculateDisplayProgress(e);
            current_task.Status = _stop_requested && e.State == NcnnTaskState.Cancelled
                ? "已停止"
                : ToTaskStatus(e.State);

            if (_last_native_state != e.State)
            {
                _last_native_state = e.State;
                AppendLog($"引擎状态: {DescribeNativeState(e.State)}");
            }

            NotifyTaskControlStateChanged();
        });
    }

    private static NcnnEngineType ToNativeEngine(EngineType engine)
    {
        return engine switch
        {
            EngineType.waifu2x => NcnnEngineType.Waifu2x,
            EngineType.RealESRGAN => NcnnEngineType.RealESRGAN,
            EngineType.srmd => NcnnEngineType.SRMD,
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, null)
        };
    }

    private static int CalculateDisplayProgress(NcnnProgressChangedEventArgs e)
    {
        if (e.State == NcnnTaskState.Completed) return 100;
        if (e.Percent > 0) return e.Percent;

        return e.State switch
        {
            NcnnTaskState.Loading => 5,
            NcnnTaskState.Processing => 50,
            NcnnTaskState.Saving => 90,
            _ => 0
        };
    }

    private static string ToTaskStatus(NcnnTaskState state)
    {
        return state switch
        {
            NcnnTaskState.Loading => "加载中",
            NcnnTaskState.Processing => "处理中",
            NcnnTaskState.Saving => "保存中",
            NcnnTaskState.Completed => "已完成",
            NcnnTaskState.Cancelled => "已取消",
            NcnnTaskState.Paused => "已暂停",
            NcnnTaskState.Failed => "失败",
            _ => "等待中"
        };
    }

    private static string DescribeNativeState(NcnnTaskState state)
    {
        return state switch
        {
            NcnnTaskState.Loading => "加载中...",
            NcnnTaskState.Processing => "处理中...",
            NcnnTaskState.Saving => "正在保存...",
            NcnnTaskState.Completed => "已完成",
            NcnnTaskState.Cancelled => "已取消",
            NcnnTaskState.Paused => "已暂停",
            NcnnTaskState.Failed => "失败",
            _ => state.ToString()
        };
    }

    private static (int Load, int Proc, int Save) ParseThreadCount(string value)
    {
        var parts = (value ?? string.Empty).Split(':', StringSplitOptions.RemoveEmptyEntries);
        var load = parts.Length > 0 ? ParseInt(parts[0], 1) : 1;
        var proc = parts.Length > 1 ? ParseInt(parts[1], 2) : 2;
        var save = parts.Length > 2 ? ParseInt(parts[2], 2) : 2;
        return (Math.Max(1, load), Math.Max(1, proc), Math.Max(1, save));
    }

    private static int ParseInt(string value, int fallback)
    {
        return int.TryParse(value, out var result) ? result : fallback;
    }


    private static bool IsTaskCompleted(TaskItem task)
    {
        return task.Progress >= 100 || task.Status == "完成";
    }

    private void MarkUnfinishedTasksStopped(TaskItem current_task = null)
    {
        foreach (var task in TaskList.Where(task => !IsTaskCompleted(task)))
            task.Status = ReferenceEquals(task, current_task) ? "正在停止" : "已停止";
    }

    private void NotifyTaskControlStateChanged()
    {
        this.RaisePropertyChanged(nameof(CanStartTask));
        this.RaisePropertyChanged(nameof(CanCancelTask));
        this.RaisePropertyChanged(nameof(CanClearTask));
    }

    private static string GetCurrentValue(Dictionary<int, string> dict, int index, string fallback = "")
    {
        return dict.GetValueOrDefault(index, fallback);
    }

    private void AppendLog(string text)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _log_builder.Append('[').Append(timestamp).Append("] ").Append(text).AppendLine();
        LogText = _log_builder.ToString();
    }

    private static async Task<IReadOnlyCollection<IStorageFile>> OpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择图片文件",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Image") { Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp"] },
                new FilePickerFileType(".jpeg") { Patterns = ["*.jpg", "*.jpeg"] },
                new FilePickerFileType(".png") { Patterns = ["*.png"] },
                new FilePickerFileType(".webp") { Patterns = ["*.webp"] }
            ]
        });
        return files.Count > 0 ? files : [];
    }
}