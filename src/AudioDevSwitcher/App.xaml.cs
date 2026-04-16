using AudioDevSwitcher.Core.Services;
using AudioDevSwitcher.Helpers;
using AudioDevSwitcher.ViewModels;
using Microsoft.UI.Xaml;

namespace AudioDevSwitcher;

public partial class App : Application
{
    private TrayIconHelper? _trayIcon;
    private MainWindow? _mainWindow;
    private IAudioDeviceService? _audioService;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _audioService = new AudioDeviceService();
        var viewModel = new MainViewModel(_audioService);

        _mainWindow = new MainWindow(viewModel);

        _trayIcon = new TrayIconHelper(_audioService, _mainWindow);
        _trayIcon.Initialize();

        // Start minimized to tray – the user interacts via the tray icon.
        // Show the window once so it's ready, then hide it.
        _mainWindow.Activate();
    }

    public void ShowMainWindow()
    {
        _mainWindow?.Activate();
    }
}
