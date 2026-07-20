using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using AudioDevSwitcher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioDevSwitcher.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IAudioDeviceService _audioService;
    private readonly ISettingsService _settingsService;
    private bool _suppressSettingsPersist;

    public ObservableCollection<AudioDeviceViewModel> OutputDevices { get; } = [];
    public ObservableCollection<AudioDeviceViewModel> InputDevices { get; } = [];

    [ObservableProperty]
    private AudioDeviceViewModel? _selectedOutputDevice;

    [ObservableProperty]
    private AudioDeviceViewModel? _selectedInputDevice;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _playConfirmationTone;

    public MainViewModel(IAudioDeviceService audioService, ISettingsService settingsService)
    {
        _audioService = audioService;
        _settingsService = settingsService;

        _audioService.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _audioService.DeviceStateChanged += OnDeviceStateChanged;

        RefreshDevices();

        _suppressSettingsPersist = true;
        StartWithWindows = WindowsStartupHelper.IsEnabled();
        StartMinimized = _settingsService.Settings.StartMinimized;
        PlayConfirmationTone = _settingsService.Settings.PlayConfirmationTone;
        _suppressSettingsPersist = false;
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_suppressSettingsPersist) return;
        WindowsStartupHelper.SetEnabled(value);
        _settingsService.Settings.StartWithWindows = value;
        _settingsService.Save();
    }

    partial void OnStartMinimizedChanged(bool value)
    {
        if (_suppressSettingsPersist) return;
        _settingsService.Settings.StartMinimized = value;
        _settingsService.Save();
    }

    partial void OnPlayConfirmationToneChanged(bool value)
    {
        if (_suppressSettingsPersist) return;
        _settingsService.Settings.PlayConfirmationTone = value;
        _settingsService.Save();
    }

    [RelayCommand]
    private void SetOutputDevice(AudioDeviceViewModel device)
    {
        if (!TrySetDefault(device.Id)) return;
        MarkDefault(OutputDevices, device.Id);
        PlayToneIfEnabled();
    }

    [RelayCommand]
    private void SetInputDevice(AudioDeviceViewModel device)
    {
        if (!TrySetDefault(device.Id)) return;
        MarkDefault(InputDevices, device.Id);
    }

    [RelayCommand]
    private void CycleOutputDevice()
    {
        var next = TryCycle(AudioDeviceType.Output);
        if (next is not null)
        {
            MarkDefault(OutputDevices, next.Id);
            PlayToneIfEnabled();
        }
    }

    [RelayCommand]
    private void CycleInputDevice()
    {
        var next = TryCycle(AudioDeviceType.Input);
        if (next is not null)
            MarkDefault(InputDevices, next.Id);
    }

    // COM failures on these paths are ordinary races — the targeted device was
    // unplugged or powered off after the list was built. Recover by
    // re-enumerating instead of letting the exception kill the app.

    private bool TrySetDefault(string deviceId)
    {
        try
        {
            _audioService.SetDefaultDevice(deviceId);
            return true;
        }
        catch (COMException)
        {
            RefreshDevices();
            return false;
        }
    }

    private AudioDevice? TryCycle(AudioDeviceType type)
    {
        try
        {
            return _audioService.CycleDevice(type);
        }
        catch (COMException)
        {
            RefreshDevices();
            return null;
        }
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        LoadDevices(AudioDeviceType.Output, OutputDevices);
        LoadDevices(AudioDeviceType.Input, InputDevices);

        SelectedOutputDevice = OutputDevices.FirstOrDefault(d => d.IsDefault);
        SelectedInputDevice = InputDevices.FirstOrDefault(d => d.IsDefault);
    }

    private void PlayToneIfEnabled()
    {
        if (PlayConfirmationTone)
            ConfirmationTonePlayer.PlayAsync();
    }

    private void LoadDevices(AudioDeviceType type, ObservableCollection<AudioDeviceViewModel> target)
    {
        IReadOnlyList<AudioDevice> devices;
        try
        {
            devices = _audioService.GetDevices(type);
        }
        catch (COMException)
        {
            // Transient enumeration failure (e.g. the audio service restarting).
            // Keep the current list; the next device notification retries.
            return;
        }

        target.Clear();
        foreach (var device in devices)
        {
            target.Add(new AudioDeviceViewModel(device));
        }
    }

    private static void MarkDefault(ObservableCollection<AudioDeviceViewModel> devices, string deviceId)
    {
        foreach (var d in devices)
            d.IsDefault = string.Equals(d.Id, deviceId, StringComparison.OrdinalIgnoreCase);
    }

    // COM notifications arrive on background threads — marshal to the UI thread.
    // Must be BeginInvoke, not Invoke: IMMNotificationClient callbacks may not
    // block, and a synchronous wait here deadlocks against a UI thread that is
    // re-entering MMDevAPI (enumeration, or Dispose's callback unregistration).

    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            var collection = e.DeviceType == AudioDeviceType.Output ? OutputDevices : InputDevices;
            MarkDefault(collection, e.DeviceId);
        });
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        Application.Current?.Dispatcher.BeginInvoke(() => RefreshDevices());
    }
}
