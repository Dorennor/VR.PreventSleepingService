using System.Diagnostics;
using System.Timers;

using Timer = System.Timers.Timer;

namespace VR.PreventSleepingService.Service
{
    public class PreventSleepingService : ServiceBase
    {
        private const string ProcessName = "VR.PreventSleepingService.Wpf";
        private static readonly string FilePath = Directory.GetCurrentDirectory() + @"\VR.PreventSleepingService.Wpf.exe";
        private static readonly string WorkingDirectory = Directory.GetCurrentDirectory();

        private readonly Timer _timer;
        private Process _preventSleepingServiceProcess;
        private bool _exited = false;

        public PreventSleepingService()
        {
            _timer = new Timer();

            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            StartupPreventSleepingService();

            _timer.Enabled = true;
        }

        private void OnExited(object? sender, EventArgs eventArgs)
        {
            _exited = true;

            _preventSleepingServiceProcess.Exited -= OnExited;
            _preventSleepingServiceProcess.Dispose();

            _preventSleepingServiceProcess = RunPreventSleepingService();
            _preventSleepingServiceProcess.EnableRaisingEvents = true;
            _preventSleepingServiceProcess.Exited += OnExited;

            _exited = false;
        }

        private void StartupPreventSleepingService()
        {
            var processes = Process.GetProcessesByName(ProcessName);

            if (processes.Length > 0)
            {
                _preventSleepingServiceProcess = processes[0];
                _preventSleepingServiceProcess.EnableRaisingEvents = true;
                _preventSleepingServiceProcess.Exited += OnExited;
            }
            else
            {
                _preventSleepingServiceProcess = RunPreventSleepingService();
                _preventSleepingServiceProcess.EnableRaisingEvents = true;
                _preventSleepingServiceProcess.Exited += OnExited;
            }
        }

        private static Process RunPreventSleepingService()
        {
            var process = new Process();

            process.StartInfo.FileName = FilePath;
            process.StartInfo.WorkingDirectory = WorkingDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.Start();

            return process;
        }

        private void TimerOnElapsed(object? sender, ElapsedEventArgs eventArgs)
        {
            var processes = Process.GetProcessesByName(ProcessName);

            if (processes.Length > 0)
            {
                if (processes[0].PriorityClass != ProcessPriorityClass.Normal)
                {
                    processes[0].PriorityClass = ProcessPriorityClass.Normal;
                }
            }
            else
            {
                StartupPreventSleepingService();
            }
        }
    }
}