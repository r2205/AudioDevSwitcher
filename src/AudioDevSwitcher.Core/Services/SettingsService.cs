using System.Text.Json;
using AudioDevSwitcher.Core.Models;

namespace AudioDevSwitcher.Core.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;

    public AppSettings Settings { get; }

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AudioDevSwitcher");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "settings.json");

        Settings = Load();
    }

    private AppSettings Load()
    {
        if (!File.Exists(_path))
            return new AppSettings();

        // A transient sharing violation (antivirus/backup/sync briefly locking
        // the file) must not be mistaken for corruption: the defaults loaded
        // here become the live Settings object, and the next Save would
        // persist them over the user's real file. Retry the read before
        // falling back.
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (JsonException)
            {
                // Genuinely corrupt content — defaults are the right answer.
                return new AppSettings();
            }
            catch (IOException) when (attempt < 3)
            {
                Thread.Sleep(100);
            }
            catch
            {
                return new AppSettings();
            }
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(_path, json);
    }
}
