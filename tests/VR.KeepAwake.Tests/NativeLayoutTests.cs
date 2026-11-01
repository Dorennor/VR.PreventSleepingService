using System.Runtime.InteropServices;

using VR.KeepAwake.Tray;

using Xunit;

namespace VR.KeepAwake.Tests;

public sealed class NativeLayoutTests
{
    [Fact]
    public void NotifyIconData_MatchesTheSizeWindowsExpects()
    {
        // A wrong size makes Shell_NotifyIcon fail, leaving the app with no tray icon.
        var expected = Environment.Is64BitProcess ? 976 : 956;

        Assert.Equal(expected, Marshal.SizeOf<NativeMethods.NOTIFYICONDATAW>());
    }
}