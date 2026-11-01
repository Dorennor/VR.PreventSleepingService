using Microsoft.Win32;
using VR.KeepAwake.Settings;
using Xunit;

namespace VR.KeepAwake.Tests;

public sealed class StartupRegistrationTests : IDisposable
{
    private const string ValueName = "Test";

    private readonly string _subKey = $@"Software\VR.KeepAwake.Tests\{Guid.NewGuid():N}";
    private readonly StartupRegistration _registration;

    public StartupRegistrationTests() => _registration = new StartupRegistration(_subKey, ValueName);

    public void Dispose() => Registry.CurrentUser.DeleteSubKeyTree(@"Software\VR.KeepAwake.Tests", throwOnMissingSubKey: false);

    [Fact]
    public void TryApply_Enabled_WritesQuotedPath()
    {
        Assert.True(_registration.TryApply(true, @"C:\Some Folder\App.exe"));

        Assert.Equal("\"C:\\Some Folder\\App.exe\"", ReadValue());
    }

    [Fact]
    public void TryApply_AfterAppMoved_UpdatesPath()
    {
        _registration.TryApply(true, @"C:\Old\App.exe");
        _registration.TryApply(true, @"C:\New\App.exe");

        Assert.Equal("\"C:\\New\\App.exe\"", ReadValue());
    }

    [Fact]
    public void TryApply_Disabled_RemovesValueAndToleratesItBeingMissing()
    {
        _registration.TryApply(true, @"C:\App.exe");

        Assert.True(_registration.TryApply(false, @"C:\App.exe"));
        Assert.True(_registration.TryApply(false, @"C:\App.exe"));
        Assert.Null(ReadValue());
    }

    private string? ReadValue()
    {
        using var key = Registry.CurrentUser.OpenSubKey(_subKey);
        return key?.GetValue(ValueName) as string;
    }
}
