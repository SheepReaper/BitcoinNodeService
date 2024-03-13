@echo off
set "scriptPath=%~dp0install.ps1"
set "serviceExecutablePath=%~dp0BitcoinNodeService.exe"
powershell.exe -ExecutionPolicy Bypass -File "%scriptPath%" -ServiceExecutablePath "%serviceExecutablePath%"
