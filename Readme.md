# Bitcoin Node Daemon Service

This service provides a convenient way to monitor and manage a Bitcoin node daemon on a Windows system. It ensures that the Bitcoin daemon (`bitcoind`) is running and provides RPC authentication credentials for secure communication.

## Features

- Automatic start of the Bitcoin node daemon (`bitcoind`) as a Windows service.
- Generation of RPC authentication credentials for secure communication with the Bitcoin node.
- Support for custom arguments passed to `bitcoind` and `bitcoin-cli`.

## Installation

1. Download the latest release from the [Releases page](https://github.com/your-repo/your-project/releases).
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

[Your License Here]

## Contributing

[Instructions for contributing to the project]