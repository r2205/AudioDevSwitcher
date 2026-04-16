using AudioDevSwitcher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace AudioDevSwitcher;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Title = "Audio Device Switcher";
        ExtendsContentIntoTitleBar = true;

        // Set window size via AppWindow (WinUI 3 doesn't support Width/Height in XAML).
        AppWindow.Resize(new SizeInt32(480, 560));

        OutputList.IsItemClickEnabled = true;
        InputList.IsItemClickEnabled = true;
        OutputList.ItemClick += OnOutputItemClick;
        InputList.ItemClick += OnInputItemClick;
    }

    private void OnOutputItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
    {
        if (e.ClickedItem is AudioDeviceViewModel device)
            ViewModel.SetOutputDeviceCommand.Execute(device);
    }

    private void OnInputItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
    {
        if (e.ClickedItem is AudioDeviceViewModel device)
            ViewModel.SetInputDeviceCommand.Execute(device);
    }
}
