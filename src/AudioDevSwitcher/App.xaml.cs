using System.Windows;
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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        _audioService = new AudioDeviceService();
        var viewModel = new MainViewModel(_audioService, _settingsService);

        _mainWindow = new MainWindow(viewModel);

        _trayIcon = new TrayIconHelper(_audioService, _mainWindow, _settingsService);
        _trayIcon.Initialize();

        bool launchHidden =
            _settingsService.Settings.StartMinimized ||
            e.Args.Any(a => string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase));

        if (!launchHidden)
            _mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _audioService?.Dispose();
        base.OnExit(e);
    }
}
