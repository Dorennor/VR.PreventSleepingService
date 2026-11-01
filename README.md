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

Copy `publish/VR.KeepAwake` to a permanent folder and run `VR.KeepAwake.exe`. This build doesn't need .NET installed on the target machine. The app registers itself to start with Windows, and you can turn that off in Settings.

If the target machine has the .NET 10 Desktop Runtime, this builds a much smaller version:

```powershell
dotnet publish src/VR.KeepAwake -c Release -r win-x64 --self-contained false -o publish/VR.KeepAwake
```

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
