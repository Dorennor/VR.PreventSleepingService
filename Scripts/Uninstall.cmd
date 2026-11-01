@echo off
rem Stops the app and removes the startup entry, the installed and unpacked files, and the settings and log.
rem No setlocal: ending it would return to the starting folder, which may be the one being deleted.

set "AppName=VR.KeepAwake"
set "ExeName=%AppName%.exe"
set "InstallDir=%LocalAppData%\Programs\%AppName%"
set "UnpackDir=%TEMP%\.net\%AppName%"
set "DataDir=%LocalAppData%\%AppName%"
set "RunKey=HKCU\Software\Microsoft\Windows\CurrentVersion\Run"

rem A folder can't be deleted while it is the current one, and a double-click starts this script inside it.
cd /d "%TEMP%"

echo Stopping the app...
call :StopApp

echo Removing from Windows startup...
reg delete "%RunKey%" /v "%AppName%" /f >nul 2>&1

echo Removing files...
if exist "%DataDir%" rmdir /s /q "%DataDir%"
if exist "%UnpackDir%" rmdir /s /q "%UnpackDir%"

rem This script deletes itself. (goto) ends it first, and the rest of the line still runs because it has already been read.
(goto) 2>nul & rmdir /s /q "%InstallDir%" 2>nul & (if exist "%InstallDir%" (echo Could not fully remove "%InstallDir%". Close anything using it and delete it manually.) else (echo %AppName% has been uninstalled.)) & pause

rem Asks the app to exit so that it removes its tray icon, and force-closes it only if it doesn't.
:StopApp
tasklist /FI "IMAGENAME eq %ExeName%" /NH | findstr /I /C:"%ExeName%" >nul || goto :eof
if exist "%InstallDir%\%ExeName%" start "" /wait "%InstallDir%\%ExeName%" --exit
for /L %%i in (1,1,5) do (
    tasklist /FI "IMAGENAME eq %ExeName%" /NH | findstr /I /C:"%ExeName%" >nul || goto :eof
    ping -n 2 127.0.0.1 >nul
)
taskkill /F /IM "%ExeName%" >nul 2>&1
ping -n 2 127.0.0.1 >nul
goto :eof
