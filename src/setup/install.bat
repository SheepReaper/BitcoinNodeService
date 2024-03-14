@echo off
set "scriptPath=%~dp0install.ps1"
powershell.exe -ExecutionPolicy Bypass -File "%scriptPath%"
