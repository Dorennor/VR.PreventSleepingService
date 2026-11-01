@echo off
rem Installs the app for the current user, which needs no admin rights. Safe to run again to update an installed copy.
setlocal

set "AppName=VR.KeepAwake"
set "ExeName=%AppName%.exe"
set "InstallDir=%LocalAppData%\Programs\%AppName%"
set "UnpackDir=%TEMP%\.net\%AppName%"
set "RunKey=HKCU\Software\Microsoft\Windows\CurrentVersion\Run"

rem Without the trailing backslash, which would escape the closing quote in the robocopy call.
set "SourceDir=%~dp0"
set "SourceDir=%SourceDir:~0,-1%"

if not exist "%SourceDir%\%ExeName%" (
    echo %ExeName% was not found next to this script. Run it from the published folder.
    goto :Fail
)

echo Stopping any running copy...
call :StopApp

rem Each build unpacks into its own subfolder, so an update would otherwise leave the old one behind.
if exist "%UnpackDir%" rmdir /s /q "%UnpackDir%"

echo Copying files to "%InstallDir%"...
robocopy "%SourceDir%" "%InstallDir%" /MIR /XF Install.cmd /R:2 /W:1 /NJH /NJS /NFL /NDL /NP >nul
rem Robocopy exit codes below 8 mean success.
if errorlevel 8 (
    echo Could not copy the files.
    goto :Fail
)

echo Adding to Windows startup...
reg add "%RunKey%" /v "%AppName%" /t REG_SZ /d "\"%InstallDir%\%ExeName%\"" /f >nul
if errorlevel 1 (
    echo Could not add the startup entry.
    goto :Fail
)

echo Starting...
start "" /D "%InstallDir%" "%InstallDir%\%ExeName%"
ping -n 3 127.0.0.1 >nul
tasklist /FI "IMAGENAME eq %ExeName%" /NH | findstr /I /C:"%ExeName%" >nul
if errorlevel 1 (
    echo The app did not start. See "%LocalAppData%\%AppName%\app.log".
    goto :Fail
)

echo.
echo Installed to "%InstallDir%".
echo The app is running in the tray. If you can't see the icon, look under the ^^ arrow in the taskbar.
echo To remove it, run Uninstall.cmd in that folder.
echo.
pause
exit /b 0

:Fail
echo.
echo Installation failed.
pause
exit /b 1

rem Asks the app to exit so that it removes its tray icon, and force-closes it only if it doesn't.
:StopApp
tasklist /FI "IMAGENAME eq %ExeName%" /NH | findstr /I /C:"%ExeName%" >nul || goto :eof
start "" /wait "%SourceDir%\%ExeName%" --exit
for /L %%i in (1,1,5) do (
    tasklist /FI "IMAGENAME eq %ExeName%" /NH | findstr /I /C:"%ExeName%" >nul || goto :eof
    ping -n 2 127.0.0.1 >nul
)
taskkill /F /IM "%ExeName%" >nul 2>&1
ping -n 2 127.0.0.1 >nul
goto :eof
