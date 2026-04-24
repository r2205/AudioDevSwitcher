using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;

namespace AudioDevSwitcher.Helpers;

/// <summary>
/// Manages the system tray icon and its context menu for quick device switching.
/// Left-click cycles the output device. Right-click shows a menu with "Open" and "Exit".
/// </summary>
public sealed class TrayIconHelper : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    private readonly IAudioDeviceService _audioService;
    private readonly Window _mainWindow;
    private readonly ISettingsService _settingsService;
    private TaskbarIcon? _trayIcon;
    private Icon? _icon;
    private IntPtr _iconHandle;

    public TrayIconHelper(IAudioDeviceService audioService, Window mainWindow, ISettingsService settingsService)
    {
        _audioService = audioService;
        _mainWindow = mainWindow;
        _settingsService = settingsService;
    }

    public void Initialize()
    {
        var showItem = new MenuItem { Header = "Open" };
        showItem.Click += (_, _) => ShowWindow();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Application.Current.Shutdown();

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);

        _icon = CreateSpeakerIcon();

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Audio Device Switcher",
            Icon = _icon,
            ContextMenu = contextMenu,
            LeftClickCommand = new RelayCommand(OnTrayLeftClick),
            DoubleClickCommand = new RelayCommand(ShowWindow),
        };

        _trayIcon.ForceCreate();

        _audioService.DefaultDeviceChanged += (_, _) => UpdateTooltip();
        UpdateTooltip();
    }

    private void ShowWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnTrayLeftClick()
    {
        var next = _audioService.CycleDevice(AudioDeviceType.Output);
        if (next is not null)
        {
            UpdateTooltip(next.Name);
            if (_settingsService.Settings.PlayConfirmationTone)
                ConfirmationTonePlayer.PlayAsync();
        }
    }

    private void UpdateTooltip(string? deviceName = null)
    {
        if (deviceName is null)
        {
            var defaultDevice = _audioService.GetDefaultDevice(AudioDeviceType.Output);
            deviceName = defaultDevice?.Name ?? "No device";
        }

        if (_trayIcon is not null)
            _trayIcon.ToolTipText = $"Output: {deviceName}";
    }

    /// <summary>
    /// Builds a 32x32 tray icon showing a speaker glyph on a blue circle.
    /// Uses Segoe MDL2 Assets (always present on Windows 10/11).
    /// </summary>
    private Icon CreateSpeakerIcon()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);

            using var bgBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
            g.FillEllipse(bgBrush, 0, 0, size - 1, size - 1);

            using var font = new System.Drawing.Font("Segoe MDL2 Assets", 16f, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            using var textBrush = new SolidBrush(Color.White);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            // U+E767 = Volume (speaker) glyph in Segoe MDL2 Assets
            g.DrawString("\uE767", font, textBrush, new RectangleF(0, 0, size, size), sf);
        }

        _iconHandle = bitmap.GetHicon();
        return Icon.FromHandle(_iconHandle);
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
        _icon?.Dispose();
        if (_iconHandle != IntPtr.Zero)
        {
            DestroyIcon(_iconHandle);
            _iconHandle = IntPtr.Zero;
        }
    }
}
