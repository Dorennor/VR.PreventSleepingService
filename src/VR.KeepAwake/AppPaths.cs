using System.IO;

namespace VR.KeepAwake;

internal static class AppPaths
{
    /// <summary>Per-user and always writable, and survives replacing the app folder with a newer build.</summary>
    public static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VR.KeepAwake");

    public static readonly string SettingsFile = Path.Combine(DataDirectory, "appsettings.json");
}