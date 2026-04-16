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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _audioService = new AudioDeviceService();
        var viewModel = new MainViewModel(_audioService);

        _mainWindow = new MainWindow(viewModel);

        _trayIcon = new TrayIconHelper(_audioService, _mainWindow);
        _trayIcon.Initialize();

        _mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _audioService?.Dispose();
        base.OnExit(e);
    }
}
