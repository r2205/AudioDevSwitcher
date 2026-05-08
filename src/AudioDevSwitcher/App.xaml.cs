using System.Windows;
using System.Windows.Interop;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using AudioDevSwitcher.Helpers;
using AudioDevSwitcher.ViewModels;

namespace AudioDevSwitcher;

public partial class App : Application
{
    private TrayIconHelper? _trayIcon;
    private MainWindow? _mainWindow;
    private IAudioDeviceService? _audioService;
    private ISettingsService? _settingsService;
    private GlobalHotkeyHelper? _hotkeys;
    private HwndSource? _hwndSource;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        _audioService = new AudioDeviceService();
        var viewModel = new MainViewModel(_audioService, _settingsService);

        _mainWindow = new MainWindow(viewModel);

        _trayIcon = new TrayIconHelper(_audioService, _mainWindow, _settingsService);
        _trayIcon.Initialize();

        // Force HWND creation so global hotkeys can be registered even when
        // the window is launched hidden (--minimized / StartMinimized).
        var interop = new WindowInteropHelper(_mainWindow);
        interop.EnsureHandle();
        _hwndSource = HwndSource.FromHwnd(interop.Handle);

        _hotkeys = new GlobalHotkeyHelper();
        _hotkeys.CycleOutputRequested += CycleOutputDevice;
        _hotkeys.CycleInputRequested += CycleInputDevice;
        _hotkeys.Register(interop.Handle);
        _hwndSource?.AddHook(_hotkeys.HookHandler);

        bool launchHidden =
            _settingsService.Settings.StartMinimized ||
            e.Args.Any(a => string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase));

        if (!launchHidden)
            _mainWindow.Show();
    }

    private void CycleOutputDevice()
    {
        var next = _audioService?.CycleDevice(AudioDeviceType.Output);
        if (next is not null && _settingsService?.Settings.PlayConfirmationTone == true)
            ConfirmationTonePlayer.PlayAsync();
    }

    private void CycleInputDevice()
    {
        _audioService?.CycleDevice(AudioDeviceType.Input);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_hwndSource is not null && _hotkeys is not null)
            _hwndSource.RemoveHook(_hotkeys.HookHandler);
        _hotkeys?.Dispose();
        _trayIcon?.Dispose();
        _audioService?.Dispose();
        base.OnExit(e);
    }
}
