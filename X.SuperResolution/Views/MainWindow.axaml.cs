using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Gpu;
using X.SuperResolution.Services;
using X.SuperResolution.ViewModels;
using static X.SuperResolution.Services.LocalizationService;

// ReSharper disable StringLiteralTypo

namespace X.SuperResolution.Views;

public partial class MainWindow : Window
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
        UserName = Environment.UserName;
        CpuCore = Environment.ProcessorCount;

        var result = DetectHardware();
        GpuList = result.GpuList;
        SysInfo = result.SysInfo;
        CpuInfo = result.CpuInfo;
        MemorySize = result.MemorySize;
        InitializeComponent();
        ThemeToggle.IsChecked = Application.Current?.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
    }

    private static HardwareInfo DetectHardware()
    {
        var computer = new Computer
        {
            IsGpuEnabled = true,
            IsCpuEnabled = true
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
                var index = 0;
                foreach (var item in hardware)
                {
                    if (item is not GenericGpu gpu)
                    {
                        continue;
                    }
                    gpu_list.Add(index, gpu.Name);
                    index++;
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

    sealed private class HardwareInfo
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

    private void ThemeToggle_OnClick(object sender, RoutedEventArgs e)
    {
        var theme = ((ToggleButton)sender).IsChecked == true ? "Dark" : "Light";
        App.ApplyTheme(theme);
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ChangeTheme(theme);
        }
    }

    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximizedState();
            return;
        }

        BeginMoveDrag(e);
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        ToggleMaximizedState();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleMaximizedState()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        var is_maximized = WindowState == WindowState.Maximized;
        MaximizeIcon.IsVisible = !is_maximized;
        RestoreIcon.IsVisible = is_maximized;
    }
}
