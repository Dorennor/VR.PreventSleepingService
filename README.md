# VR.KeepAwake

A small Windows tray app that stops the laptop from sleeping or showing the idle screen saver and lock screen while long-running tasks run.

## Usage

- **Left-click the tray icon** to turn it on or off. A sun means on, and a moon means Windows behaves as usual.
- **Right-click the tray icon** for the menu: on/off, Settings and Exit.

Turn it off before leaving the laptop somewhere public, so that it locks itself as usual.

Windows can still sleep if you close the lid or press the power button.

## Install

```powershell
dotnet publish src/VR.KeepAwake -p:PublishProfile=win-x64
```

In Visual Studio, use **Publish** on the project and pick the `win-x64` profile.

Then run `Install.cmd` from `publish/VR.KeepAwake`, which also works after copying that folder to another machine. The build is a single exe that doesn't need .NET installed on the target machine. It isn't compressed, because that would cost about 70 MB more memory while running. On first start, it unpacks WPF's native libraries to `%TEMP%\.net\VR.KeepAwake` (about 8 MB).

The script doesn't need admin rights. It:

- copies the app to `%LocalAppData%\Programs\VR.KeepAwake`, replacing any earlier version
- adds the app to Windows startup, which you can turn off in Settings
- starts the app, hidden in the tray

Run `Install.cmd` from a newer build to update. Your settings are kept.

If the target machine has the .NET 10 Desktop Runtime, this builds a much smaller version:

```powershell
dotnet publish src/VR.KeepAwake -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish/VR.KeepAwake
```

## Uninstall

Run `Uninstall.cmd` in `%LocalAppData%\Programs\VR.KeepAwake`. It stops the app and removes the startup entry, the installed and unpacked files, and the settings and log.

## Files

Both files are in `%LocalAppData%\VR.KeepAwake`:

- `appsettings.json` stores the last on/off state and the autostart setting.
- `app.log` records starts, state changes and errors.

## Check that it's working

In an elevated terminal, run `powercfg /requests`. `VR.KeepAwake.exe` should be listed under `DISPLAY` and `SYSTEM`.

## Development

```powershell
dotnet build
dotnet test
```

The keep awake call in `Power/KeepAwakeGuard.cs` is the combination proven on the work laptop. Test any change to it there.
