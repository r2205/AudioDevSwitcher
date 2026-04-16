using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        var showCommand = new RelayCommand(() => _mainWindow.Activate());
        var exitCommand = new RelayCommand(() => Application.Current.Exit());

        var contextMenu = new MenuFlyout();
        contextMenu.Items.Add(new MenuFlyoutItem { Text = "Open", Command = showCommand });
        contextMenu.Items.Add(new MenuFlyoutSeparator());
        contextMenu.Items.Add(new MenuFlyoutItem { Text = "Exit", Command = exitCommand });

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Audio Device Switcher",
            LeftClickCommand = new RelayCommand(OnTrayLeftClick),
            DoubleClickCommand = showCommand,
            ContextFlyout = contextMenu,
        };

        _trayIcon.ForceCreate();

        _audioService.DefaultDeviceChanged += (_, _) => UpdateTooltip();
        UpdateTooltip();
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
