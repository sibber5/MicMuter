@echo off
setlocal enabledelayedexpansion

dotnet clean ./src/MicMuter.csproj -c Release
dotnet publish ./src/MicMuter.csproj -r win-x64 -c Release -o publish

pause
