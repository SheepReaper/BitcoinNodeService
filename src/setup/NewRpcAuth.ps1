<#
.SYNOPSIS
Generates RPC authentication credentials for a Bitcoin node.

.DESCRIPTION
This script creates a username and password for JSON-RPC authentication
for a Bitcoin node and generates the corresponding HMAC for inclusion
in the bitcoin.conf file or appsettings.json. The password can be either
provided as a SecureString, generated randomly, or input interactively.

.LINK
This script was adapted from the python version of it distributed with bitcoin core.
https://github.com/bitcoin/bitcoin/blob/master/share/rpcauth/rpcauth.py
https://bitcoin.org/en/developer-reference#rpcauth

.PARAMETER Username
The username for authentication. This parameter is mandatory.

.PARAMETER Password
The password for authentication as a SecureString. If not provided, the
script will prompt for a password or generate a random one based on the
GeneratePassword switch.

.PARAMETER GeneratePassword
A switch to indicate that a random password should be generated. If this
switch is provided, the Password parameter should not be used.

.PARAMETER Script
A switch to indicate that only the result object is to be written to the output. 
Use when called from other scripts or pipelining.

.EXAMPLE
.\ScriptName.ps1 -Username myuser

Prompts the user to enter a password and generates HMAC for the specified username.

.EXAMPLE
.\ScriptName.ps1 -Username myuser -GeneratePassword

Generates a random password and HMAC for the specified username.

.EXAMPLE
$securePassword = ConvertTo-SecureString "mypassword" -AsPlainText -Force
.\ScriptName.ps1 -Username myuser -Password $securePassword

Uses the specified SecureString password to generate the HMAC for the specified username.

.NOTES
The generated password is a 32-byte Base64 encoded string. The script ensures that
the password is handled securely by using SecureString and not allowing direct input
of plain text passwords.
#>
[CmdletBinding(DefaultParameterSetName = "GeneratePassword")]
param(
    [Parameter(Mandatory = $true)]
    [string]$Username,

    [Parameter(Mandatory = $false)]
    [switch]$Script,

    [Parameter(Mandatory = $true, ParameterSetName = "DirectInputPassword")]
    [System.Security.SecureString]$Password,

    [Parameter(Mandatory = $false, ParameterSetName = "GeneratePassword")]
    [switch]$GeneratePassword
)

function New-Salt {
    param(
        [int]$Size
    )

    return ( -join ((1..$Size) | ForEach-Object { Get-Random -Maximum 256 | ForEach-Object { "{0:X2}" -f $_ } })).ToLower()
}

function New-Password {
    # Create a 32-byte Base64 encoded password
    $bytes = New-Object byte[] 24

    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)

    $base64String = [Convert]::ToBase64String($bytes)

    # Convert the base64 string to a SecureString
    $secureString = New-Object System.Security.SecureString
    $base64String.ToCharArray() | ForEach-Object { $secureString.AppendChar($_) }

    return $secureString
}

function ConvertTo-HMAC {
    param (
        [string]$Salt,
        [System.Security.SecureString]$Password
    )

    # Convert SecureString to byte array
    $passwordBstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)

    try {
        $passwordBytes = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordBstr)
        $passwordBytes = [System.Text.Encoding]::UTF8.GetBytes($passwordBytes)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordBstr)
    }

    # Compute HMAC
    $hmac = New-Object System.Security.Cryptography.HMACSHA256
    $hmac.Key = [System.Text.Encoding]::UTF8.GetBytes($Salt)
    $hashBytes = $hmac.ComputeHash($passwordBytes)

    return [BitConverter]::ToString($hashBytes).Replace("-", "").ToLower()
}

$Password ??= $GeneratePassword ? (New-Password) : (Read-Host "Enter password" -AsSecureString)

# Convert SecureString to plain text only when necessary
if ($Password -is [System.Security.SecureString]) {
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)

    try {
        $PlainTextPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}
else {
    $PlainTextPassword = $Password
}

# Create a 16-byte hex salt
$Salt = New-Salt 16
$PasswordHMAC = ConvertTo-HMAC $Salt $Password

$output = [PSCustomObject]@{
    "BitcoinDArgs"   = @("-rpcauth=${Username}:$Salt`$$PasswordHMAC")
    "BitcoinCliArgs" = @("-rpcuser=$Username", "-rpcpassword=$PlainTextPassword")
}

if ($Script) {
    Write-Output $output
}
else {
    Write-Host "Add the following string to 'BitcoinD.StartArgs[]' in appsettings.json OR bitcoin.conf"
    Write-Host $output.BitcoinDArgs[0]
    Write-Host "`nAdd the following strings to 'BitcoinCli.StartArgs[]' in appsettings.json"
    Write-Host $output.BitcoinCliArgs[0]
    Write-Host $output.BitcoinCliArgs[1]
    Write-Host "`nPress any key to exit..."

    $null = [System.Console]::ReadKey()
}
