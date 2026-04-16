using System.Runtime.InteropServices;
using AudioDevSwitcher.Core.Interop;
using AudioDevSwitcher.Core.Models;

namespace AudioDevSwitcher.Core.Services;

/// <summary>
/// Manages audio endpoint devices via Windows Core Audio COM APIs.
/// Provides enumeration, default-device switching, and change notifications.
/// </summary>
public sealed class AudioDeviceService : IAudioDeviceService, IMMNotificationClient
{
    private readonly IMMDeviceEnumerator _enumerator;
    private readonly IPolicyConfig _policyConfig;
    private bool _disposed;

    public event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;
    public event EventHandler<DeviceStateChangedEventArgs>? DeviceStateChanged;

    public AudioDeviceService()
    {
        _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorFactory();
        _policyConfig = (IPolicyConfig)new PolicyConfigClientFactory();
        _enumerator.RegisterEndpointNotificationCallback(this);
    }

    public IReadOnlyList<AudioDevice> GetDevices(AudioDeviceType type)
    {
        var flow = type == AudioDeviceType.Output ? EDataFlow.eRender : EDataFlow.eCapture;

        int hr = _enumerator.EnumAudioEndpoints(flow, DeviceState.Active, out var collection);
        Marshal.ThrowExceptionForHR(hr);

        hr = collection.GetCount(out int count);
        Marshal.ThrowExceptionForHR(hr);

        string? defaultId = GetDefaultDeviceId(flow);

        var devices = new List<AudioDevice>(count);
        for (int i = 0; i < count; i++)
        {
            hr = collection.Item(i, out var mmDevice);
            Marshal.ThrowExceptionForHR(hr);

            var device = BuildAudioDevice(mmDevice, type, defaultId);
            if (device is not null)
                devices.Add(device);
        }

        return devices;
    }

    public AudioDevice? GetDefaultDevice(AudioDeviceType type)
    {
        var flow = type == AudioDeviceType.Output ? EDataFlow.eRender : EDataFlow.eCapture;

        int hr = _enumerator.GetDefaultAudioEndpoint(flow, ERole.eMultimedia, out var mmDevice);
        if (hr != 0) return null; // No default device

        return BuildAudioDevice(mmDevice, type, defaultId: null, forceDefault: true);
    }

    public void SetDefaultDevice(string deviceId)
    {
        // Set for all three roles so the device becomes the default everywhere.
        Marshal.ThrowExceptionForHR(_policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole));
        Marshal.ThrowExceptionForHR(_policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia));
        Marshal.ThrowExceptionForHR(_policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications));
    }

    public AudioDevice? CycleDevice(AudioDeviceType type)
    {
        var devices = GetDevices(type);
        if (devices.Count <= 1) return devices.FirstOrDefault();

        int currentIndex = -1;
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i].IsDefault)
            {
                currentIndex = i;
                break;
            }
        }

        int nextIndex = (currentIndex + 1) % devices.Count;
        var next = devices[nextIndex];

        SetDefaultDevice(next.Id);
        return next;
    }

    // ── IMMNotificationClient implementation ────────────────────────────

    void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string? defaultDeviceId)
    {
        // Only fire once per flow change (use eMultimedia as the canonical role).
        if (role != ERole.eMultimedia) return;

        var type = flow == EDataFlow.eRender ? AudioDeviceType.Output : AudioDeviceType.Input;
        DefaultDeviceChanged?.Invoke(this, new DefaultDeviceChangedEventArgs
        {
            DeviceId = defaultDeviceId ?? string.Empty,
            DeviceType = type,
        });
    }

    void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs
        {
            DeviceId = deviceId,
            IsActive = newState == DeviceState.Active,
        });
    }

    void IMMNotificationClient.OnDeviceAdded(string deviceId)
    {
        DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs
        {
            DeviceId = deviceId,
            IsActive = true,
        });
    }

    void IMMNotificationClient.OnDeviceRemoved(string deviceId)
    {
        DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs
        {
            DeviceId = deviceId,
            IsActive = false,
        });
    }

    void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key)
    {
        // We treat property changes as state changes so the UI can refresh.
        DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs
        {
            DeviceId = deviceId,
            IsActive = true,
        });
    }

    // ── Private helpers ─────────────────────────────────────────────────

    private string? GetDefaultDeviceId(EDataFlow flow)
    {
        int hr = _enumerator.GetDefaultAudioEndpoint(flow, ERole.eMultimedia, out var device);
        if (hr != 0) return null;

        device.GetId(out string id);
        return id;
    }

    private static AudioDevice? BuildAudioDevice(
        IMMDevice mmDevice, AudioDeviceType type, string? defaultId, bool forceDefault = false)
    {
        mmDevice.GetId(out string id);
        mmDevice.GetState(out var state);

        int hr = mmDevice.OpenPropertyStore(0 /* STGM_READ */, out var props);
        if (hr != 0) return null;

        var nameKey = PropertyKeys.DeviceFriendlyName;
        props.GetValue(ref nameKey, out var nameProp);
        string name = nameProp.AsString();

        if (string.IsNullOrWhiteSpace(name))
        {
            var descKey = PropertyKeys.DeviceDescription;
            props.GetValue(ref descKey, out var descProp);
            name = descProp.AsString();
        }

        if (string.IsNullOrWhiteSpace(name))
            name = id;

        return new AudioDevice
        {
            Id = id,
            Name = name,
            Type = type,
            IsDefault = forceDefault || string.Equals(id, defaultId, StringComparison.OrdinalIgnoreCase),
            IsActive = state == DeviceState.Active,
        };
    }

    // ── Disposal ────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _enumerator.UnregisterEndpointNotificationCallback(this);
    }
}
