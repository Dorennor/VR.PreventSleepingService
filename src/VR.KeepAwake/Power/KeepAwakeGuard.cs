using System.Runtime.InteropServices;

namespace VR.KeepAwake.Power;

/// <summary>
/// Keeps the display and system awake, which also stops the idle screen saver and its lock screen.
/// </summary>
/// <remarks>
/// These flags and the dedicated thread are the combination proven on the work laptop, so change them
/// only after testing there. Windows ties the request to the calling thread and drops it when that
/// thread ends, so the thread stays blocked (using no CPU) for as long as the request is active.
/// </remarks>
internal sealed partial class KeepAwakeGuard : IDisposable
{
    internal const ExecutionState RequestedState =
        ExecutionState.Continuous | ExecutionState.DisplayRequired | ExecutionState.SystemRequired;

    private Thread? _thread;
    private ManualResetEventSlim? _stop;

    public bool IsActive => _thread is not null;

    /// <returns><see langword="true"/> if Windows accepted the request.</returns>
    public bool Activate()
    {
        if (_thread is not null)
        {
            return true;
        }

        var stop = new ManualResetEventSlim();
        using var requested = new ManualResetEventSlim();
        var accepted = false;

        var thread = new Thread(() =>
        {
            accepted = SetThreadExecutionState(RequestedState) != 0;
            requested.Set();

            if (accepted)
            {
                stop.Wait();
                SetThreadExecutionState(ExecutionState.Continuous);
            }
        })
        {
            IsBackground = true,
            Name = "KeepAwake",
        };

        thread.Start();
        requested.Wait();

        if (!accepted)
        {
            stop.Dispose();
            return false;
        }

        _thread = thread;
        _stop = stop;
        return true;
    }

    public void Deactivate()
    {
        if (_thread is null)
        {
            return;
        }

        _stop!.Set();
        _thread.Join();
        _stop.Dispose();
        _stop = null;
        _thread = null;
    }

    public void Dispose() => Deactivate();

    [LibraryImport("kernel32.dll")]
    private static partial uint SetThreadExecutionState(ExecutionState state);
}
