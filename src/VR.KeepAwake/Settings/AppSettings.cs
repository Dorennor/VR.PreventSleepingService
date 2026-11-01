using System.Text.Json.Serialization;

namespace VR.KeepAwake.Settings;

internal sealed record AppSettings
{
    // Plain setters: with init, the JSON source generator replaces the default of a missing property with false.
    public bool KeepAwakeEnabled { get; set; } = true;

    public bool StartWithWindows { get; set; } = true;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class AppSettingsJsonContext : JsonSerializerContext;