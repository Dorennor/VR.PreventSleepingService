using VR.KeepAwake.Tray;

using Xunit;

namespace VR.KeepAwake.Tests;

public sealed class EmbeddedIconTests
{
    [Theory]
    [InlineData("tray-on.ico", 16)]
    [InlineData("tray-on.ico", 24)]
    [InlineData("tray-on.ico", 32)]
    [InlineData("tray-off.ico", 16)]
    [InlineData("tray-off.ico", 24)]
    [InlineData("tray-off.ico", 32)]
    public void Load_TrayIcon_ReturnsHandle(string resourceName, int size)
    {
        var icon = EmbeddedIcon.Load(resourceName, size);

        Assert.NotEqual(0, icon);
        NativeMethods.DestroyIcon(icon);
    }

    [Fact]
    public void Load_UnknownResource_ReturnsZero()
    {
        Assert.Equal(0, EmbeddedIcon.Load("missing.ico", 16));
    }
}