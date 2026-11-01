using System.IO;

namespace VR.KeepAwake;

/// <summary>Small event log, used to diagnose problems on machines that can't be debugged directly.</summary>
internal static class AppLog
{
    private const long MaxBytes = 256 * 1024;

    private static readonly string FilePath = Path.Combine(AppPaths.DataDirectory, "app.log");
    private static readonly Lock Gate = new();

    public static void Write(string message)
    {
        // Logging must never be the reason the app stops working.
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(AppPaths.DataDirectory);

                var file = new FileInfo(FilePath);
                if (file.Exists && file.Length > MaxBytes)
                {
                    file.Delete();
                }

                File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}