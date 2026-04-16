namespace AudioDevSwitcher.Core.Models;

/// <summary>
/// Represents an audio endpoint device (speaker, headphones, microphone, etc.).
/// </summary>
public sealed class AudioDevice
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public AudioDeviceType Type { get; init; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; init; }

    public override string ToString() => $"{Name} ({Type}{(IsDefault ? ", Default" : "")})";
}
