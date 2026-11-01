using System.IO;
using VR.KeepAwake.Settings;
using Xunit;

namespace VR.KeepAwake.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("VR.KeepAwake.Tests").FullName;

    private string FilePath => Path.Combine(_directory, "nested", "appsettings.json");

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public void Load_WithoutFile_ReturnsDefaultsWithBothEnabled()
    {
        var settings = new SettingsStore(FilePath).Load();

        Assert.True(settings.KeepAwakeEnabled);
        Assert.True(settings.StartWithWindows);
    }

    [Fact]
    public void Save_ThenLoad_ReturnsLastSavedSettings()
    {
        var store = new SettingsStore(FilePath);
        var saved = new AppSettings { KeepAwakeEnabled = false, StartWithWindows = false };

        store.Save(new AppSettings());
        store.Save(saved);

        Assert.Equal(saved, store.Load());
        Assert.False(File.Exists(FilePath + ".tmp"));
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{\"KeepAwakeEnabled\": \"maybe\"}")]
    public void Load_WithDamagedFile_ReturnsDefaults(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, content);

        Assert.Equal(new AppSettings(), new SettingsStore(FilePath).Load());
    }

    [Fact]
    public void Load_WithMissingProperty_KeepsItsDefault()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, "{\"KeepAwakeEnabled\": false}");

        var settings = new SettingsStore(FilePath).Load();

        Assert.False(settings.KeepAwakeEnabled);
        Assert.True(settings.StartWithWindows);
    }
}
