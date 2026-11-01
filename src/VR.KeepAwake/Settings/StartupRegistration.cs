using System.IO;
using System.Security;

using Microsoft.Win32;

namespace VR.KeepAwake.Settings;

/// <summary>Starts the app at sign-in through the per-user Run key, which needs no admin rights.</summary>
internal sealed class StartupRegistration(
    string subKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run",
    string valueName = "VR.KeepAwake")
{
    /// <summary>Also repairs the entry if the app folder has moved since it was written.</summary>
    /// <returns><see langword="true"/> if the registry now matches <paramref name="enabled"/>.</returns>
    public bool TryApply(bool enabled, string exePath)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(subKeyPath);

            if (!enabled)
            {
                key.DeleteValue(valueName, throwOnMissingValue: false);
                return true;
            }

            var command = $"\"{exePath}\"";
            if (!string.Equals(key.GetValue(valueName) as string, command, StringComparison.OrdinalIgnoreCase))
            {
                key.SetValue(valueName, command);
            }

            return true;
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException)
        {
            AppLog.Write($"Could not update autostart: {ex.Message}");
            return false;
        }
    }
}