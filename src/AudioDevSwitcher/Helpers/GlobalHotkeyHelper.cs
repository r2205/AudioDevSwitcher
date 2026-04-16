using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;

namespace AudioDevSwitcher.Helpers;

/// <summary>
/// Registers system-wide hotkeys so the user can switch audio devices
/// without bringing the app to the foreground.
/// Default: Ctrl+Alt+PageUp cycles output, Ctrl+Alt+PageDown cycles input.
/// </summary>
public sealed class GlobalHotkeyHelper : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_ALT = 0x0001;
    private const int VK_PRIOR = 0x21;  // Page Up
    private const int VK_NEXT = 0x22;   // Page Down

    private const int HOTKEY_CYCLE_OUTPUT = 1;
    private const int HOTKEY_CYCLE_INPUT = 2;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _hwnd;
    private bool _registered;

    public event Action? CycleOutputRequested;
    public event Action? CycleInputRequested;

    /// <summary>
    /// Registers global hotkeys. Call after the main window has an HWND.
    /// </summary>
    public void Register(IntPtr hwnd)
    {
        _hwnd = hwnd;

        _registered =
            RegisterHotKey(hwnd, HOTKEY_CYCLE_OUTPUT, MOD_CONTROL | MOD_ALT, VK_PRIOR) &&
            RegisterHotKey(hwnd, HOTKEY_CYCLE_INPUT, MOD_CONTROL | MOD_ALT, VK_NEXT);
    }

    /// <summary>
    /// Call from a window message hook or subclass procedure when WM_HOTKEY is received.
    /// </summary>
    public void HandleHotkeyMessage(int hotkeyId)
    {
        switch (hotkeyId)
        {
            case HOTKEY_CYCLE_OUTPUT:
                CycleOutputRequested?.Invoke();
                break;
            case HOTKEY_CYCLE_INPUT:
                CycleInputRequested?.Invoke();
                break;
        }
    }

    public void Dispose()
    {
        if (_registered && _hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, HOTKEY_CYCLE_OUTPUT);
            UnregisterHotKey(_hwnd, HOTKEY_CYCLE_INPUT);
            _registered = false;
        }
    }
}
