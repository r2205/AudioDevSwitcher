using System.Windows;
using AudioDevSwitcher.ViewModels;

namespace AudioDevSwitcher;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        OutputList.MouseDoubleClick += (_, _) =>
        {
            if (OutputList.SelectedItem is AudioDeviceViewModel device)
                ViewModel.SetOutputDeviceCommand.Execute(device);
        };

        InputList.MouseDoubleClick += (_, _) =>
        {
            if (InputList.SelectedItem is AudioDeviceViewModel device)
                ViewModel.SetInputDeviceCommand.Execute(device);
        };
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
