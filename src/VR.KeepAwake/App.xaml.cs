using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using VR.KeepAwake.Settings;
using VR.KeepAwake.Tray;
using VR.KeepAwake.Views;

using Wpf.Ui.Appearance;
using Wpf.Ui.Markup;

namespace VR.KeepAwake;

public partial class App : Application
{
    /// <summary>Used by the install scripts to close the running copy.</summary>
    private const string ExitArgument = "--exit";

    private Mutex? _singleInstance;
    private AppController? _controller;
    private TrayIcon? _tray;
    private SettingsWindow? _settingsWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Contains(ExitArgument))
        {
            TrayIcon.RequestExit();
            Shutdown();
            return;
        }

        _singleInstance = new Mutex(initiallyOwned: true, @"Local\VR.KeepAwake", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) => AppLog.Write($"Fatal error: {args.ExceptionObject}");

        AppLog.Write("Started.");

        _controller = new AppController(
            new SettingsStore(AppPaths.SettingsFile),
            new StartupRegistration(),
            Environment.ProcessPath!);
        _controller.PropertyChanged += (_, _) => _tray?.SetActive(_controller.KeepAwakeEnabled);

        _tray = new TrayIcon(_controller.KeepAwakeEnabled);
        _tray.ToggleRequested += () => _controller.KeepAwakeEnabled = !_controller.KeepAwakeEnabled;
        _tray.SettingsRequested += ShowSettings;
        _tray.ExitRequested += Shutdown;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _controller?.Dispose();
        _singleInstance?.Dispose();

        base.OnExit(e);
    }

    /// <summary>The window and its WPF UI resources exist only while open, so they cost nothing the rest of the time.</summary>
    private void ShowSettings()
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        // Keeps the GPU drivers out of the process, as they can't be unloaded once the window has closed.
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

        var dictionaries = Resources.MergedDictionaries;
        var theme = new ThemesDictionary();
        var controls = new ControlsDictionary();
        dictionaries.Add(theme);
        dictionaries.Add(controls);
        ApplicationThemeManager.ApplySystemTheme();

        _settingsWindow = new SettingsWindow(_controller!);
        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;
            dictionaries.Clear();

            // Waits until the window is fully gone. Aggressive mode returns the freed memory to Windows.
            Dispatcher.BeginInvoke(
                () => GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive),
                DispatcherPriority.ContextIdle);
        };
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    /// <summary>A failure in the UI must not end the process, because that would also end the keep awake request.</summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppLog.Write($"Unhandled error: {e.Exception}");
        e.Handled = true;
    }
}