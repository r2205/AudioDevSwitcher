using System.Runtime.InteropServices;

namespace AudioDevSwitcher.Core.Interop;

// ── Data flow & state enums ──────────────────────────────────────────

internal enum EDataFlow
{
    eRender = 0,
    eCapture = 1,
    eAll = 2,
}

internal enum ERole
{
    eConsole = 0,
    eMultimedia = 1,
    eCommunications = 2,
}

[Flags]
internal enum DeviceState : uint
{
    Active = 0x00000001,
    Disabled = 0x00000002,
    NotPresent = 0x00000004,
    Unplugged = 0x00000008,
    All = 0x0000000F,
}

// ── PROPERTYKEY ──────────────────────────────────────────────────────

[StructLayout(LayoutKind.Sequential)]
internal struct PropertyKey
{
    public Guid FormatId;
    public int PropertyId;

    public PropertyKey(Guid formatId, int propertyId)
    {
        FormatId = formatId;
        PropertyId = propertyId;
    }
}

// ── PROPVARIANT (simplified – we only read string values) ────────────

[StructLayout(LayoutKind.Sequential)]
internal struct PropVariant
{
    public ushort VarType;
    private ushort _reserved1;
    private ushort _reserved2;
    private ushort _reserved3;
    public IntPtr Value;

    public readonly string AsString()
    {
        // VT_LPWSTR = 31
        if (VarType == 31 && Value != IntPtr.Zero)
            return Marshal.PtrToStringUni(Value) ?? string.Empty;
        return string.Empty;
    }
}

// ── IMMDeviceEnumerator ──────────────────────────────────────────────

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    [PreserveSig]
    int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice device);

    [PreserveSig]
    int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);

    [PreserveSig]
    int RegisterEndpointNotificationCallback(IMMNotificationClient client);

    [PreserveSig]
    int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
}

// ── IMMDeviceCollection ──────────────────────────────────────────────

[ComImport]
[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceCollection
{
    [PreserveSig]
    int GetCount(out int count);

    [PreserveSig]
    int Item(int index, out IMMDevice device);
}

// ── IMMDevice ────────────────────────────────────────────────────────

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    [PreserveSig]
    int Activate(
        [In] ref Guid interfaceId,
        [In] int classContext,
        [In] IntPtr activationParams,
        [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

    [PreserveSig]
    int OpenPropertyStore(int stgmAccess, out IPropertyStore properties);

    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

    [PreserveSig]
    int GetState(out DeviceState state);
}

// ── IPropertyStore ───────────────────────────────────────────────────

[ComImport]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out int count);

    [PreserveSig]
    int GetAt(int index, out PropertyKey key);

    [PreserveSig]
    int GetValue(ref PropertyKey key, out PropVariant value);

    [PreserveSig]
    int SetValue(ref PropertyKey key, ref PropVariant value);

    [PreserveSig]
    int Commit();
}

// ── IMMNotificationClient ────────────────────────────────────────────

[ComImport]
[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMNotificationClient
{
    void OnDeviceStateChanged(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        DeviceState newState);

    void OnDeviceAdded(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    void OnDeviceRemoved(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    void OnDefaultDeviceChanged(
        EDataFlow flow,
        ERole role,
        [MarshalAs(UnmanagedType.LPWStr)] string? defaultDeviceId);

    void OnPropertyValueChanged(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        PropertyKey key);
}

// ── IPolicyConfig (undocumented – used to set default audio device) ──

[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    // We only need SetDefaultEndpoint – stubs for vtable ordering.
    [PreserveSig]
    int GetMixFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr format);

    [PreserveSig]
    int GetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        int param1,
        IntPtr format);

    [PreserveSig]
    int ResetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    [PreserveSig]
    int SetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr format,
        IntPtr mixFormat);

    [PreserveSig]
    int GetProcessingPeriod(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        int param1,
        IntPtr defaultPeriod,
        IntPtr minimumPeriod);

    [PreserveSig]
    int SetProcessingPeriod(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr period);

    [PreserveSig]
    int GetShareMode(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr mode);

    [PreserveSig]
    int SetShareMode(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr mode);

    [PreserveSig]
    int GetPropertyValue(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        PropertyKey key,
        out PropVariant value);

    [PreserveSig]
    int SetPropertyValue(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        PropertyKey key,
        ref PropVariant value);

    [PreserveSig]
    int SetDefaultEndpoint(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        ERole role);

    [PreserveSig]
    int SetEndpointVisibility(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        int isVisible);
}

// ── COM class factories ──────────────────────────────────────────────

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumeratorFactory
{
}

[ComImport]
[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
internal class PolicyConfigClientFactory
{
}
