namespace BitcoinNodeService;

internal sealed partial class DaemonMonitorService
{
    [LoggerMessage(101, LogLevel.Critical, "Path supplied for {BinName} ({Path}) was not found.")]
    public partial void LogFileNotFound(string BinName, string Path);

    [LoggerMessage(102, LogLevel.Critical, "Bitcoind has failed to start.")]
    public partial void LogResultBitcoinDStartFail();

    [LoggerMessage(103, LogLevel.Critical, "'{BinPath}' failed to start.")]
    public partial void LogExecFailCritical(string BinPath);

    [LoggerMessage(201, LogLevel.Error, "Was unable to send the stop command.")]
    public partial void LogErrorCliExec();
    
    [LoggerMessage(202, LogLevel.Error, "'{BinPath}' failed to start.")]
    public partial void LogExecFailError(string BinPath);

    [LoggerMessage(202, LogLevel.Error, "Daemon shutdown timeout exceeded.")]
    public partial void LogErrorTimeoutExceeded();

    [LoggerMessage(203, LogLevel.Error, "Epic failure! Daemon is still running.")]
    public partial void LogErrorDaemonRunning();

    [LoggerMessage(301, LogLevel.Warning, "Bitcoind has stopped on its own.")]
    public partial void LogAbnormalBitcoinDStopped();

    [LoggerMessage(302, LogLevel.Warning, "'{BinPath}' startup reused existing process.")]
    public partial void LogUsedExistingProcess(string BinPath);

    [LoggerMessage(401, LogLevel.Information, "Bitcoin Node Service is monitoring bitcoind and waiting for a signal to shut it down.")]
    public partial void LogStateNormal();

    [LoggerMessage(402, LogLevel.Information, "Bitcoin Node Service is starting bitcoind...")]
    public partial void LogStateServiceMain();

    [LoggerMessage(403, LogLevel.Information, "Bitcoind has started.")]
    public partial void LogResultBitcoinDStartSuccess();

    [LoggerMessage(404, LogLevel.Information, "Bitcoin Node Service has been asked to stop the daemon.")]
    public partial void LogStateServiceStopping();

    [LoggerMessage(405, LogLevel.Information, "Waiting for stop command to complete...")]
    public partial void LogStateStoppingBitcoinD();

    [LoggerMessage(406, LogLevel.Information, "Bitcoin Node Service is stopping.")]
    public partial void LogStateServiceComplete();

    [LoggerMessage(407, LogLevel.Information, "Waiting for daemon to stop...")]
    public partial void LogStateWaitingDaemonStopped();

    [LoggerMessage(501, LogLevel.Debug, "Executing: '{Path} {Args}'")]
    public partial void LogExec(string Path, string Args);
}
