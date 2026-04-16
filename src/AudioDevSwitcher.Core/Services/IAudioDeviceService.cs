using AudioDevSwitcher.Core.Models;

namespace AudioDevSwitcher.Core.Services;

public interface IAudioDeviceService : IDisposable
{
    /// <summary>Returns all active audio devices of the given type.</summary>
    IReadOnlyList<AudioDevice> GetDevices(AudioDeviceType type);

    /// <summary>Returns the current default device for the given type, or null if none.</summary>
    AudioDevice? GetDefaultDevice(AudioDeviceType type);

    /// <summary>Sets the given device as the default for all audio roles.</summary>
    void SetDefaultDevice(string deviceId);

    /// <summary>Cycles to the next active device of the given type and returns it.</summary>
    AudioDevice? CycleDevice(AudioDeviceType type);

    /// <summary>Raised when the default device changes (by this app or externally).</summary>
    event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;

    /// <summary>Raised when a device is added, removed, or its state changes.</summary>
    event EventHandler<DeviceStateChangedEventArgs>? DeviceStateChanged;
}
