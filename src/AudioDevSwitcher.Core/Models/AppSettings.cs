namespace AudioDevSwitcher.Core.Models;

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; }
    public bool StartMinimized { get; set; }
    public bool PlayConfirmationTone { get; set; } = true;
}
