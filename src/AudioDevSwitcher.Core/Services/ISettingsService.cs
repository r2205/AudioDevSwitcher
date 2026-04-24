using AudioDevSwitcher.Core.Models;

namespace AudioDevSwitcher.Core.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
    void Save();
}
