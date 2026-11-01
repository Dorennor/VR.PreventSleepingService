using System.ComponentModel;
using System.Runtime.CompilerServices;
using VR.KeepAwake.Power;
using VR.KeepAwake.Settings;

namespace VR.KeepAwake;

/// <summary>Single source of truth for the app state, shared by the tray icon and the settings window.</summary>
internal sealed class AppController : INotifyPropertyChanged, IDisposable
{
    private readonly KeepAwakeGuard _guard = new();
    private readonly SettingsStore _store;
    private readonly StartupRegistration _startup;
    private readonly string _exePath;
    private bool _startWithWindows;

    public AppController(SettingsStore store, StartupRegistration startup, string exePath)
    {
        _store = store;
        _startup = startup;
        _exePath = exePath;

        var settings = store.Load();
        _startWithWindows = settings.StartWithWindows;

        if (!_startup.TryApply(_startWithWindows, _exePath))
        {
            _startWithWindows = false;
        }

        if (settings.KeepAwakeEnabled)
        {
            ActivateGuard();
        }

        Save();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool KeepAwakeEnabled
    {
        get => _guard.IsActive;
        set
        {
            if (value == _guard.IsActive)
            {
                return;
            }

            if (value)
            {
                ActivateGuard();
            }
            else
            {
                _guard.Deactivate();
                AppLog.Write("Keep awake off.");
            }

            Save();
            OnPropertyChanged();
        }
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (value == _startWithWindows)
            {
                return;
            }

            if (_startup.TryApply(value, _exePath))
            {
                _startWithWindows = value;
                Save();
            }

            OnPropertyChanged();
        }
    }

    public void Dispose() => _guard.Dispose();

    private void ActivateGuard()
    {
        AppLog.Write(_guard.Activate() ? "Keep awake on." : "Windows rejected the keep awake request.");
    }

    private void Save() => _store.Save(new AppSettings
    {
        KeepAwakeEnabled = KeepAwakeEnabled,
        StartWithWindows = _startWithWindows,
    });

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
