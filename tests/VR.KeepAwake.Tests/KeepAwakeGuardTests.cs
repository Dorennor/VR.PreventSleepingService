using System.Runtime.InteropServices;
using VR.KeepAwake.Power;
using Xunit;

namespace VR.KeepAwake.Tests;

public sealed partial class KeepAwakeGuardTests
{
    private const uint RequiredFlags = (uint)(ExecutionState.DisplayRequired | ExecutionState.SystemRequired);

    [Fact]
    public void RequestedState_IsTheCombinationProvenOnTheWorkLaptop()
    {
        Assert.Equal(0x80000003u, (uint)KeepAwakeGuard.RequestedState);
    }

    [Fact]
    public void Activate_MakesWindowsReportDisplayAndSystemRequired()
    {
        using var guard = new KeepAwakeGuard();

        Assert.True(guard.Activate());
        Assert.True(guard.IsActive);
        Assert.Equal(RequiredFlags, ReadSystemExecutionState() & RequiredFlags);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_KeepsTheSameRequest()
    {
        using var guard = new KeepAwakeGuard();

        Assert.True(guard.Activate());
        Assert.True(guard.Activate());
        Assert.True(guard.IsActive);
    }

    [Fact]
    public void Deactivate_CanBeFollowedByActivate()
    {
        using var guard = new KeepAwakeGuard();

        guard.Deactivate();
        Assert.False(guard.IsActive);

        guard.Activate();
        guard.Deactivate();
        Assert.False(guard.IsActive);

        Assert.True(guard.Activate());
        Assert.Equal(RequiredFlags, ReadSystemExecutionState() & RequiredFlags);
    }

    private static uint ReadSystemExecutionState()
    {
        Assert.Equal(0, CallNtPowerInformation(SystemExecutionState, 0, 0, out var state, sizeof(uint)));
        return state;
    }

    private const int SystemExecutionState = 16;

    [LibraryImport("powrprof.dll")]
    private static partial int CallNtPowerInformation(int level, nint input, uint inputLength, out uint output, uint outputLength);
}
