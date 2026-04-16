using AudioDevSwitcher.ViewModels;
using Microsoft.UI.Xaml;

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

        OutputList.ItemClick += OnOutputItemClick;
        InputList.ItemClick += OnInputItemClick;
        OutputList.IsItemClickEnabled = true;
        InputList.IsItemClickEnabled = true;
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
