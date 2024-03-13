#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory = $false)]
    [string]$ServiceExecutablePath
)

# Get the full path to the BitcoinNodeService.exe
if (-not $ServiceExecutablePath) {
    $ServiceExecutablePath = Resolve-Path ".\BitcoinNodeService.exe"
}

# Use NetworkService so it can run at boot without login
$networkServiceAccount = "NT AUTHORITY\NetworkService"
$networkServicePassword = ConvertTo-SecureString -String "doesn't matter" -AsPlainText -Force
$networkServiceCredential = New-Object System.Management.Automation.PSCredential ($networkServiceAccount, $networkServicePassword)

# Create the service to run automatically at boot under the Network Service account
$service = New-Service -Name "Bitcoin Node Daemon" `
    -BinaryPathName $ServiceExecutablePath `
    -DisplayName "Bitcoin Node Service" `
    -StartupType Automatic `
    -Description "Service for monitoring and managing the Bitcoin node daemon." `
    -Credential $networkServiceCredential
    
Write-Host "Bitcoin Node Service has been installed successfully and is set to start automatically."
    
# Prompt for RPC credentials and update appsettings.json
$response = Read-Host "Would you like to generate RPC credentials and update appsettings.json? (Y/N)"

if ($response -match '^(y|yes)$') {
    $username = Read-Host "Enter the RPC username"
    $rpcInfo = & ".\NewRpcAuth.ps1" -Username $username -GeneratePassword -Script
    $appSettings = Get-Content "appsettings.json" | ConvertFrom-Json
    $appSettings.BitcoinD.StartArgs += $rpcInfo.BitcoinDArgs
    $appSettings.BitcoinCli.StartArgs += $rpcInfo.BitcoinCliArgs
    $appSettings | ConvertTo-Json | Set-Content "appsettings.json"

    Write-Host "appsettings.json has been updated with the new RPC credentials."
}

# Prompt to start the service
$startResponse = Read-Host "Would you like to start the service now? (Y/N)"

if ($startResponse -match '^(y|yes)$') {
    $service.Start()
    
    Write-Host "Bitcoin Node Service has been started."
}

Write-Host "Press any key to exit..."

$null = [System.Console]::ReadKey()
