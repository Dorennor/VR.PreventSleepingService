using System.Runtime.InteropServices;

namespace VR.PowerUtilities
{
    public static partial class PowerUtilities
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial uint SetThreadExecutionState(ExecutionState esFlags);

        private static readonly AutoResetEvent Event = new AutoResetEvent(false);

        public static void PreventPowerSave()
        {
            new TaskFactory().StartNew(() =>
                {
                    SetThreadExecutionState(ExecutionState.EsContinuous |
                                            ExecutionState.EsDisplayRequired |
                                            ExecutionState.EsSystemRequired
                    );

                    Event.WaitOne();
                },
                TaskCreationOptions.LongRunning
            );
        }

        public static void Shutdown()
        {
            Event.Set();
        }
    }
}