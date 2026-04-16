using System.Windows;
using System.Windows.Controls;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;

namespace AudioDevSwitcher.Helpers;

/// <summary>
/// Manages the system tray icon and its context menu for quick device switching.
/// Left-click cycles the output device. Right-click shows a menu with "Open" and "Exit".
/// </summary>
public sealed class TrayIconHelper : IDisposable
{
    private readonly IAudioDeviceService _audioService;
    private readonly Window _mainWindow;
    private TaskbarIcon? _trayIcon;

    public TrayIconHelper(IAudioDeviceService audioService, Window mainWindow)
    {
        _audioService = audioService;
        _mainWindow = mainWindow;
    }

    public void Initialize()
    {
        var showItem = new MenuItem { Header = "Open" };
        showItem.Click += (_, _) => ShowWindow();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Application.Current.Shutdown();

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Audio Device Switcher",
            ContextMenu = contextMenu,
            LeftClickCommand = new RelayCommand(OnTrayLeftClick),
            DoubleClickCommand = new RelayCommand(ShowWindow),
        };

        _trayIcon.ForceCreate();

        _audioService.DefaultDeviceChanged += (_, _) => UpdateTooltip();
        UpdateTooltip();
    }

    private void ShowWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnTrayLeftClick()
    {
        var next = _audioService.CycleDevice(AudioDeviceType.Output);
        if (next is not null)
            UpdateTooltip(next.Name);
    }

    private void UpdateTooltip(string? deviceName = null)
    {
        if (deviceName is null)
        {
            var defaultDevice = _audioService.GetDefaultDevice(AudioDeviceType.Output);
            deviceName = defaultDevice?.Name ?? "No device";
        }

        if (_trayIcon is not null)
            _trayIcon.ToolTipText = $"Output: {deviceName}";
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
