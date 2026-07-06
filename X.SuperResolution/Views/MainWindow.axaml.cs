using Avalonia;
using Avalonia.Controls;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Gpu;
using SukiUI;
using SukiUI.Controls;
using X.SuperResolution.Services;
using X.SuperResolution.ViewModels;
using static X.SuperResolution.Services.LocalizationService;

// ReSharper disable StringLiteralTypo

namespace X.SuperResolution.Views;

public partial class MainWindow : SukiWindow
{
    /// <summary>
    /// GPU列表
    /// </summary>
    public Dictionary<int, string> GpuList { get; set; } = [];

    /// <summary>
    /// 系统信息
    /// </summary>
    public SystemInformation SysInfo { get; set; }

    /// <summary>
    /// 处理器信息
    /// </summary>
    public ProcessorInformation CpuInfo { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 核心数量
    /// </summary>
    public int CpuCore { get; set; }

    /// <summary>
    /// 内存大小
    /// </summary>
    public string MemorySize { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        UserName = Environment.UserName;
        CpuCore = Environment.ProcessorCount;

        var result = DetectHardware();
        GpuList = result.GpuList;
        SysInfo = result.SysInfo;
        CpuInfo = result.CpuInfo;
        MemorySize = result.MemorySize;
    }

    private static HardwareInfo DetectHardware()
    {
        var computer = new Computer
        {
            IsGpuEnabled = true
        };
        try
        {
            computer.Open();
            computer.Accept(new UpdateVisitor());
            var info = computer.SMBios;
            var processor = info.Processors.FirstOrDefault();
            var memory = $"{info.MemoryDevices.Sum(x => x.Size) / 1024M:F0}GB";
            var gpu_list = new Dictionary<int, string>();
            var hardware = computer.Hardware;
            if (hardware is { Count: > 0 })
            {
                for (var i = 0; i < hardware.Count; i++)
                {
                    gpu_list.Add(i, ((GenericGpu)hardware[i]).Name);
                }
            }

            return new HardwareInfo
            {
                GpuList = gpu_list,
                SysInfo = info.System,
                CpuInfo = processor,
                MemorySize = memory
            };
        }
        finally
        {
            computer.Close();
        }
    }

    private sealed class HardwareInfo
    {
        public Dictionary<int, string> GpuList { get; init; }
        public SystemInformation SysInfo { get; init; }
        public ProcessorInformation CpuInfo { get; init; }
        public string MemorySize { get; init; }
    }

    private void Lang_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var code = ((ComboBox)sender).SelectedValue!.ToString();
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        ApplyLanguage(code);
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ChangeLanguage(code);
        }
    }

    private void InfoTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        InfoTextBox.CaretIndex = InfoTextBox.Text?.Length ?? 0;
    }
}
