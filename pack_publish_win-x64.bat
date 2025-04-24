@echo off
setlocal enabledelayedexpansion

CALL ./publish_win-x64.bat --no-pause

for /f "delims=" %%i in (pack_id.txt) do (
    set packId=%%i
    goto :done
)
:done

for /f %%v in ('powershell -command "(Get-Item './publish/win-x64/MicMuter.dll').VersionInfo.FileVersion"') do set fileVersion=%%v
for /f "tokens=1-3 delims=." %%a in ("%fileVersion%") do set version=%%a.%%b.%%c

echo Packing %packId% v%version%

vpk pack --packTitle MicMuter --packAuthors sibber5 --icon ./src/Assets/icon_unmuted.ico --packId %packId% --packVersion %version% --packDir ./publish/win-x64 --outputDir ./publish_installer/win-x64 --mainExe MicMuter.exe --framework net9.0-x64-desktop

if "%1" NEQ "--no-pause" pause
