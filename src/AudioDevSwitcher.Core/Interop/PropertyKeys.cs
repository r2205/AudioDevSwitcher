namespace AudioDevSwitcher.Core.Interop;

internal static class PropertyKeys
{
    private static readonly Guid DevicePropertyGuid =
        new("A45C254E-DF1C-4EFD-8020-67D146A850E0");

    private static readonly Guid DeviceInterfacePropertyGuid =
        new("233164C8-1B2C-4C7D-BC68-B671687A2567");

    /// <summary>
    /// Friendly name of the audio endpoint (e.g. "Speakers (Realtek Audio)").
    /// </summary>
    public static PropertyKey DeviceFriendlyName =>
        new(DevicePropertyGuid, 14);

    /// <summary>
    /// Device description (e.g. "Speakers").
    /// </summary>
    public static PropertyKey DeviceDescription =>
        new(DevicePropertyGuid, 2);

    /// <summary>
    /// Interface friendly name (e.g. "Realtek Audio").
    /// </summary>
    public static PropertyKey DeviceInterfaceFriendlyName =>
        new(DeviceInterfacePropertyGuid, 2);
}
