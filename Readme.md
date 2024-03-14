[![CodeQL](https://github.com/SheepReaper/BitcoinNodeService/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/SheepReaper/BitcoinNodeService/actions/workflows/github-code-scanning/codeql)
[![Release Build](https://github.com/SheepReaper/BitcoinNodeService/actions/workflows/release.yml/badge.svg?event=release)](https://github.com/SheepReaper/BitcoinNodeService/actions/workflows/release.yml)

# Bitcoin Node Daemon Service

This service provides a convenient way to monitor and manage a Bitcoin node daemon on a Windows system. It ensures that the Bitcoin daemon (`bitcoind`) is running and provides RPC authentication credentials for secure communication.

# Why?

Starting bitcoind for the first time takes ages to complete the initial block download. A little known bottleneck on windows is that writing to the Console is a blocking operation. The synchronization process is actually slowed down significntly by writing to it. Launching bitcoind manually, explicitly without a window speeds up the process significantly. From there, I had the itch to wrap it into a service that could start automatically, and even run unattended.

## Features

- Automatic start of the Bitcoin node daemon (`bitcoind`) as a Windows service.
- Generation of RPC authentication credentials for secure communication with the Bitcoin node.
- Support for custom arguments passed to `bitcoind` and `bitcoin-cli`.

## Requirements

- bitcoind and bitcoin-cli. Both are provided by installing [bitcoin core](https://bitcoin.org/en/download)
- If you plan on using the unattended startup, install bitcoin core and this application to a location that is not within a User directory, specifically, it needs to be accessible to NT AUTHORITY\NetworkService. This is an identity with minimal privilege, one step above Local Service, so that it can access the network.

## Additional Steps

- This documentation only covers the wrapper service. bitcoind requires additional configuration. Please reference [bitcoin.org](https://developer.bitcoin.org/) for complete documentation of bitcoind.
- Any command line arguments you would need to pass to either bitcoind or bitcoin-cli can be instead placed in appsettings.json.

## Installation

1. Download the latest release from the [Releases page](https://github.com/SheepReaper/BitcoinNodeService/releases).
2. Extract the contents of the downloaded ZIP archive to a desired location. **(You may not change this location after the service is installed!)**
4. Right-Click `install.bat` and select Run as Administrator to install the Bitcoin Node Daemon service. The install script is interactive and will prompt you to generate RPC credentials if desired. It will also offer to start the service for the first time.

   ```cmd
   install.bat
   ```

## Configuration

Before starting the service, ensure that the `appsettings.json` file is updated correctly for your system. The most important properties to configure are:

- `BitcoinD.BinPath`: The file path to the `bitcoind` executable.
- `BitcoinD.StartArgs[]`: An array of arguments to pass to the `bitcoind` executable.
- `BitcoinCli.BinPath`: The file path to the `bitcoin-cli` executable.
- `BitcoinCli.StartArgs[]`: An array of arguments to pass to the `bitcoin-cli` executable.

Each argument is passed literally to the executables, so any arguments they support are also supported by this service.

## Uninstallation

To uninstall the Bitcoin Node Daemon service, Right-Click `uninstall.bat` and select Run as Administrator:

```cmd
uninstall.bat
```

## Troubleshooting

- If the service does not start after installation, verify that the `appsettings.json` file is correctly configured with the correct paths and arguments for `bitcoind` and `bitcoin-cli`.
- Ensure that the service is installed and run with Administrator privileges.

## License

[MIT](https://github.com/SheepReaper/BitcoinNodeService?tab=MIT-1-ov-file)

## Contributing

[Contributing](https://github.com/SheepReaper/.github/blob/main/CONTRIBUTING.md)
