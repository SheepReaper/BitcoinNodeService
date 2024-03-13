#Requires -RunAsAdministrator

# Define the service name
$serviceName = "Bitcoin Node Daemon"

# Check if the service exists
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    # Stop the service if it's running
    $service = Get-Service -Name $serviceName

    if ($service.Status -eq 'Running') {
        Stop-Service -Name $serviceName -Force
        Write-Host "Bitcoin Node Service has been stopped."
    }

    # Remove the service
    $service | Remove-Service

    Write-Host "Bitcoin Node Service has been uninstalled successfully."
} else {
    Write-Host "Bitcoin Node Service does not exist or has already been uninstalled."
}

Write-Host "Press any key to exit..."

$null = [System.Console]::ReadKey()
