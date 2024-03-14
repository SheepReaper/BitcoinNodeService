#Requires -RunAsAdministrator

# Get the directory of the current script
$scriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Get the full path to the BitcoinNodeService.exe
$ServiceExecutablePath = Join-Path -Path $scriptDirectory -ChildPath "BitcoinNodeService.exe"

# Use NetworkService so it can run at boot without login
$networkServiceAccount = "NT AUTHORITY\NetworkService"

# Create the service to run automatically at boot under the Network Service account
$null = New-Service -Name "Bitcoin Node Daemon" `
    -BinaryPathName $ServiceExecutablePath `
    -DisplayName "Bitcoin Node Service" `
    -StartupType Automatic `
    -Description "Service for monitoring and managing the Bitcoin node daemon."
    
# Set the service to use the NetworkService account
sc.exe config "Bitcoin Node Daemon" obj= $networkServiceAccount password= "doesn't matter"

Write-Host "Bitcoin Node Service has been installed successfully and is set to start automatically."
    
# Prompt for RPC credentials and update appsettings.json
$response = Read-Host "Would you like to generate RPC credentials and update appsettings.json? (Y/N)"

if ($response -match '^(y|yes)$') {
    $username = Read-Host "Enter the RPC username"
    $rpcScriptPath = Join-Path -Path $scriptDirectory -ChildPath "NewRpcAuth.ps1"
    $rpcInfo = & $rpcScriptPath -Username $username -GeneratePassword -Script
    $appSettingsPath = Join-Path -Path $scriptDirectory -ChildPath "appsettings.json"
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    $appSettings.BitcoinD.StartArgs += $rpcInfo.BitcoinDArgs
    $appSettings.BitcoinCli.StartArgs += $rpcInfo.BitcoinCliArgs
    $appSettings | ConvertTo-Json -Depth 3 | Set-Content $appSettingsPath

    Write-Host "appsettings.json has been updated with the new RPC credentials."
}

# Prompt to start the service
$startResponse = Read-Host "Would you like to start the service now? (Y/N)"

if ($startResponse -match '^(y|yes)$') {
    Start-Service -Name "Bitcoin Node Daemon"
    
    Write-Host "Bitcoin Node Service has been started."
}

Write-Host "Press any key to exit..."

$null = [System.Console]::ReadKey()
