using AudioDevSwitcher.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioDevSwitcher.ViewModels;

public sealed partial class AudioDeviceViewModel : ObservableObject
{
    public string Id { get; }
    public string Name { get; }
    public AudioDeviceType Type { get; }

    [ObservableProperty]
    private bool _isDefault;

    public AudioDeviceViewModel(AudioDevice device)
    {
        Id = device.Id;
        Name = device.Name;
        Type = device.Type;
        IsDefault = device.IsDefault;
    }
}
