using System.IO;
using System.Text.Json;

namespace VR.KeepAwake.Settings;

internal sealed class SettingsStore(string filePath)
{
    /// <summary>Never throws: a missing or damaged file must not stop the app, so it falls back to defaults.</summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new AppSettings();
            }

            return JsonSerializer.Deserialize(File.ReadAllText(filePath), AppSettingsJsonContext.Default.AppSettings)
                ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppLog.Write($"Could not read settings, using defaults: {ex.Message}");
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Written to a temp file first so a crash or power loss can't leave a half-written file.
            var tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings));
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppLog.Write($"Could not save settings: {ex.Message}");
        }
    }
}