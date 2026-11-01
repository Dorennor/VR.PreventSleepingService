using System.Runtime.InteropServices;
using System.Windows.Threading;
using static VR.KeepAwake.Tray.NativeMethods;

namespace VR.KeepAwake.Tray;

/// <summary>
/// Tray icon on plain Win32 calls with a native menu. Any WPF window, even a hidden one, starts the render
/// engine, which loads the GPU drivers into the process, so that waits until the settings window opens.
/// </summary>
internal sealed class TrayIcon : IDisposable
{
    private const string WindowClassName = "VR.KeepAwake.Tray";
    private const int CallbackMessage = WM_APP + 1;
    private const uint IconId = 1;

    private const int MenuToggle = 1;
    private const int MenuSettings = 2;
    private const int MenuExit = 3;

    private const int MaxAddAttempts = 30;
    private static readonly TimeSpan AddRetryInterval = TimeSpan.FromSeconds(2);

    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private readonly nint _instance = GetModuleHandle(null);
    private readonly uint _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
    private readonly WndProc _wndProc;
    private readonly nint _window;
    private readonly nint _iconOn;
    private readonly nint _iconOff;
    private readonly DispatcherTimer _addRetryTimer;
    private bool _isActive;
    private bool _isAdded;
    private int _addAttempts;

    public TrayIcon(bool isActive)
    {
        _isActive = isActive;

        // Kept in a field because Windows calls it for as long as the window exists.
        _wndProc = OnWindowMessage;
        _window = CreateHiddenWindow();

        var size = GetSystemMetricsForDpi(SM_CXSMICON, GetDpiForWindow(_window));
        _iconOn = LoadTrayIcon("tray-on.ico", size);
        _iconOff = LoadTrayIcon("tray-off.ico", size);

        _addRetryTimer = new DispatcherTimer { Interval = AddRetryInterval };
        _addRetryTimer.Tick += (_, _) => AddIcon();

        AddIcon();
    }

    public event Action? ToggleRequested;

    public event Action? SettingsRequested;

    public event Action? ExitRequested;

    /// <summary>Asks the running copy to exit, so that it removes its icon. Does nothing if there isn't one.</summary>
    public static void RequestExit()
    {
        var window = FindWindow(WindowClassName, null);
        if (window != 0)
        {
            PostMessage(window, WM_CLOSE, 0, 0);
        }
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;

        if (_isAdded)
        {
            var data = CreateData();
            if (!Shell_NotifyIcon(NIM_MODIFY, ref data))
            {
                AppLog.Write("Could not update the tray icon.");
            }
        }
    }

    public void Dispose()
    {
        _addRetryTimer.Stop();

        if (_isAdded)
        {
            var data = CreateData();
            Shell_NotifyIcon(NIM_DELETE, ref data);
        }

        DestroyIcon(_iconOn);
        DestroyIcon(_iconOff);
        DestroyWindow(_window);
        UnregisterClass(WindowClassName, _instance);
    }

    private static nint LoadTrayIcon(string fileName, int size)
    {
        var icon = EmbeddedIcon.Load(fileName, size);
        if (icon != 0)
        {
            return icon;
        }

        // Without any icon there would be nothing to click to turn the app off or exit it.
        AppLog.Write($"Could not load {fileName}, using the default icon.");
        return LoadIcon(0, IDI_APPLICATION);
    }

    /// <summary>A hidden top-level window: message-only windows don't receive the TaskbarCreated broadcast.</summary>
    private unsafe nint CreateHiddenWindow()
    {
        fixed (char* className = WindowClassName)
        {
            var windowClass = new WNDCLASSEXW
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                hInstance = _instance,
                lpszClassName = className,
            };

            RegisterClassEx(windowClass);
        }

        var window = CreateWindowEx(WS_EX_TOOLWINDOW, WindowClassName, WindowClassName, WS_POPUP, 0, 0, 0, 0, 0, 0, _instance, 0);
        if (window == 0)
        {
            throw new InvalidOperationException("Could not create the tray window.");
        }

        return window;
    }

    /// <summary>The shell may not be ready during sign-in, when autostart runs, so failures are retried.</summary>
    private void AddIcon()
    {
        var data = CreateData();

        // The icon can already exist: TaskbarCreated also fires on display scaling changes, and a timed-out add may have worked.
        Shell_NotifyIcon(NIM_DELETE, ref data);
        _isAdded = Shell_NotifyIcon(NIM_ADD, ref data);

        if (_isAdded)
        {
            _addAttempts = 0;
            _addRetryTimer.Stop();
            return;
        }

        if (++_addAttempts < MaxAddAttempts)
        {
            _addRetryTimer.Start();
            return;
        }

        // Explorer sends TaskbarCreated when it next starts, which tries again.
        AppLog.Write("Could not add the tray icon.");
        _addAttempts = 0;
        _addRetryTimer.Stop();
    }

    private unsafe NOTIFYICONDATAW CreateData()
    {
        var data = new NOTIFYICONDATAW
        {
            cbSize = (uint)sizeof(NOTIFYICONDATAW),
            hWnd = _window,
            uID = IconId,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = CallbackMessage,
            hIcon = _isActive ? _iconOn : _iconOff,
        };

        var tip = _isActive ? "VR.KeepAwake: on" : "VR.KeepAwake: off";
        tip.AsSpan().CopyTo(new Span<char>(data.szTip, 127));

        return data;
    }

    private nint OnWindowMessage(nint window, uint message, nint wParam, nint lParam)
    {
        // An exception must not escape into native code, as that would kill the process.
        try
        {
            if (message == CallbackMessage)
            {
                switch ((int)(lParam & 0xFFFF))
                {
                    case WM_LBUTTONUP:
                        Post(ToggleRequested);
                        break;
                    case WM_RBUTTONUP:
                        ShowMenu();
                        break;
                }

                return 0;
            }

            if (message == _taskbarCreatedMessage)
            {
                _addAttempts = 0;
                AddIcon();
                return 0;
            }

            // By default this would destroy the window and leave the process running without an icon.
            if (message == WM_CLOSE)
            {
                Post(ExitRequested);
                return 0;
            }
        }
        catch (Exception ex)
        {
            AppLog.Write($"Tray error: {ex}");
            return 0;
        }

        return DefWindowProc(window, message, wParam, lParam);
    }

    /// <summary>Runs outside the native callback, where the normal error handling of the app applies.</summary>
    private void Post(Action? action)
    {
        if (action is not null)
        {
            _dispatcher.BeginInvoke(action);
        }
    }

    private void ShowMenu()
    {
        var menu = CreatePopupMenu();
        AppendMenu(menu, MF_STRING | (_isActive ? MF_CHECKED : 0), MenuToggle, "Keep screen awake");
        AppendMenu(menu, MF_STRING, MenuSettings, "Settings...");
        AppendMenu(menu, MF_SEPARATOR, 0, null);
        AppendMenu(menu, MF_STRING, MenuExit, "Exit");

        GetCursorPos(out var cursor);

        // Both calls around the menu are required for it to close when clicking elsewhere.
        SetForegroundWindow(_window);
        var command = TrackPopupMenuEx(menu, TPM_RETURNCMD | TPM_NONOTIFY | TPM_RIGHTBUTTON, cursor.X, cursor.Y, _window, 0);
        PostMessage(_window, WM_NULL, 0, 0);
        DestroyMenu(menu);

        switch (command)
        {
            case MenuToggle:
                Post(ToggleRequested);
                break;
            case MenuSettings:
                Post(SettingsRequested);
                break;
            case MenuExit:
                Post(ExitRequested);
                break;
        }
    }
}
