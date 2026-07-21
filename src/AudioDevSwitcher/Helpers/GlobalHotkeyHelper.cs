using System.Runtime.InteropServices;

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

    // Display strings for UI hints; keep in sync with the Register calls below.
    public const string CycleOutputGesture = "Ctrl+Alt+PgUp";
    public const string CycleInputGesture = "Ctrl+Alt+PgDn";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _hwnd;
    private bool _outputRegistered;
    private bool _inputRegistered;

    public event Action? CycleOutputRequested;
    public event Action? CycleInputRequested;

    /// <summary>
    /// Registers global hotkeys. Call after the main window has an HWND.
    /// </summary>
    public void Register(IntPtr hwnd)
    {
        _hwnd = hwnd;

        // Register and track each hotkey independently: a conflict on one combo
        // (another app already owns it) must not cost the user the other one,
        // and Dispose must unregister exactly what was registered.
        _outputRegistered = RegisterHotKey(hwnd, HOTKEY_CYCLE_OUTPUT, MOD_CONTROL | MOD_ALT, VK_PRIOR);
        _inputRegistered = RegisterHotKey(hwnd, HOTKEY_CYCLE_INPUT, MOD_CONTROL | MOD_ALT, VK_NEXT);
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

    /// <summary>
    /// HwndSource hook callback. Pass to <c>HwndSource.AddHook</c>.
    /// </summary>
    public IntPtr HookHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            HandleHotkeyMessage(wParam.ToInt32());
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_hwnd == IntPtr.Zero) return;

        if (_outputRegistered)
        {
            UnregisterHotKey(_hwnd, HOTKEY_CYCLE_OUTPUT);
            _outputRegistered = false;
        }

        if (_inputRegistered)
        {
            UnregisterHotKey(_hwnd, HOTKEY_CYCLE_INPUT);
            _inputRegistered = false;
        }
    }
}
