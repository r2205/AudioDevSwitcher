namespace AudioDevSwitcher.Core.Models;

public sealed class DefaultDeviceChangedEventArgs : EventArgs
{
    public required string DeviceId { get; init; }
    public required AudioDeviceType DeviceType { get; init; }
}

public sealed class DeviceStateChangedEventArgs : EventArgs
{
    public required string DeviceId { get; init; }
    public required bool IsActive { get; init; }
}
