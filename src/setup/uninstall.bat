@echo off
set "scriptPath=%~dp0uninstall.ps1"
powershell.exe -ExecutionPolicy Bypass -File "%scriptPath%"
