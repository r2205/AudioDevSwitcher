using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AudioDevSwitcher.Helpers;
using AudioDevSwitcher.ViewModels;

namespace AudioDevSwitcher;

public partial class MainWindow : Window
{
    public const string AppName = "David's Audio Device Switcher";

    public static string AppVersion
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is not null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "v0.0.0";
        }
    }

    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Title = AppName;
        VersionLabel.Text = AppVersion;
        OutputHotkeyHint.Text = $"Cycle: {GlobalHotkeyHelper.CycleOutputGesture}";
        InputHotkeyHint.Text = $"Cycle: {GlobalHotkeyHelper.CycleInputGesture}";

        OutputList.MouseDoubleClick += (_, e) =>
        {
            if (GetDoubleClickedDevice(e) is AudioDeviceViewModel device)
                ViewModel.SetOutputDeviceCommand.Execute(device);
        };

        InputList.MouseDoubleClick += (_, e) =>
        {
            if (GetDoubleClickedDevice(e) is AudioDeviceViewModel device)
                ViewModel.SetInputDeviceCommand.Execute(device);
        };
    }

    /// <summary>
    /// Returns the device row a double-click landed on, or null. MouseDoubleClick
    /// also fires for right-button double-clicks and for clicks on the list's
    /// empty area or scrollbar; only a left-button double-click on an actual
    /// row may switch devices.
    /// </summary>
    private static AudioDeviceViewModel? GetDoubleClickedDevice(MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return null;

        for (var element = e.OriginalSource as DependencyObject; element is not null;
             element = element is Visual ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is ListViewItem item)
                return item.DataContext as AudioDeviceViewModel;
        }

        return null;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        // Minimize to tray instead of taskbar.
        if (WindowState == WindowState.Minimized)
            Hide();

        base.OnStateChanged(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Hide to tray instead of closing. Use the tray "Exit" to actually quit.
        e.Cancel = true;
        Hide();
    }
}
