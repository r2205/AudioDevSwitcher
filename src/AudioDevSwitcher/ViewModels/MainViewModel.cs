using System.Collections.ObjectModel;
using System.Windows;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioDevSwitcher.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IAudioDeviceService _audioService;

    public ObservableCollection<AudioDeviceViewModel> OutputDevices { get; } = [];
    public ObservableCollection<AudioDeviceViewModel> InputDevices { get; } = [];

    [ObservableProperty]
    private AudioDeviceViewModel? _selectedOutputDevice;

    [ObservableProperty]
    private AudioDeviceViewModel? _selectedInputDevice;

    public MainViewModel(IAudioDeviceService audioService)
    {
        _audioService = audioService;

        _audioService.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _audioService.DeviceStateChanged += OnDeviceStateChanged;

        RefreshDevices();
    }

    [RelayCommand]
    private void SetOutputDevice(AudioDeviceViewModel device)
    {
        _audioService.SetDefaultDevice(device.Id);
        MarkDefault(OutputDevices, device.Id);
    }

    [RelayCommand]
    private void SetInputDevice(AudioDeviceViewModel device)
    {
        _audioService.SetDefaultDevice(device.Id);
        MarkDefault(InputDevices, device.Id);
    }

    [RelayCommand]
    private void CycleOutputDevice()
    {
        var next = _audioService.CycleDevice(AudioDeviceType.Output);
        if (next is not null)
            MarkDefault(OutputDevices, next.Id);
    }

    [RelayCommand]
    private void CycleInputDevice()
    {
        var next = _audioService.CycleDevice(AudioDeviceType.Input);
        if (next is not null)
            MarkDefault(InputDevices, next.Id);
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        LoadDevices(AudioDeviceType.Output, OutputDevices);
        LoadDevices(AudioDeviceType.Input, InputDevices);

        SelectedOutputDevice = OutputDevices.FirstOrDefault(d => d.IsDefault);
        SelectedInputDevice = InputDevices.FirstOrDefault(d => d.IsDefault);
    }

    private void LoadDevices(AudioDeviceType type, ObservableCollection<AudioDeviceViewModel> target)
    {
        target.Clear();
        foreach (var device in _audioService.GetDevices(type))
        {
            target.Add(new AudioDeviceViewModel(device));
        }
    }

    private static void MarkDefault(ObservableCollection<AudioDeviceViewModel> devices, string deviceId)
    {
        foreach (var d in devices)
            d.IsDefault = string.Equals(d.Id, deviceId, StringComparison.OrdinalIgnoreCase);
    }

    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        // COM notifications arrive on background threads — marshal to the UI thread.
        Application.Current.Dispatcher.Invoke(() =>
        {
            var collection = e.DeviceType == AudioDeviceType.Output ? OutputDevices : InputDevices;
            MarkDefault(collection, e.DeviceId);
        });
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => RefreshDevices());
    }
}
