@echo off
setlocal enabledelayedexpansion

dotnet clean ./MicMuter/MicMuter.csproj -c Release
dotnet publish ./MicMuter/MicMuter.csproj -r win-x64 -c Release -o publish

pause
