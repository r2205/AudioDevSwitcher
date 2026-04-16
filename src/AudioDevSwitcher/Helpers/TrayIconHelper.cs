using System.Runtime.InteropServices;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Xaml;

namespace AudioDevSwitcher.Helpers;

/// <summary>
/// Manages the system tray icon and its context menu for quick device switching.
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
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Audio Device Switcher",
        };

        // Use the application icon embedded in the exe; falls back to a default.
        _trayIcon.ForceCreate();

        _trayIcon.ContextMenuMode = H.NotifyIcon.ContextMenuMode.SecondWindow;
        _trayIcon.TrayLeftMouseDown += OnTrayLeftClick;
        _trayIcon.TrayRightMouseDown += OnTrayRightClick;

        _audioService.DefaultDeviceChanged += (_, _) => UpdateTooltip();
        UpdateTooltip();
    }

    private void OnTrayLeftClick(object? sender, RoutedEventArgs e)
    {
        // Left-click cycles the output device for fast switching.
        var next = _audioService.CycleDevice(AudioDeviceType.Output);
        if (next is not null)
            UpdateTooltip(next.Name);
    }

    private void OnTrayRightClick(object? sender, RoutedEventArgs e)
    {
        _mainWindow.Activate();
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
