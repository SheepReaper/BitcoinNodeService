using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Options;

namespace BitcoinNodeService;

internal sealed partial class DaemonMonitorService(
    IOptions<BitcoinDOptions> dOptions,
    IOptions<BitcoinCliOptions> cliOptions,
    IHostApplicationLifetime lifeTime,
    ILogger<DaemonMonitorService> logger
) : BackgroundService
{
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ILogger<DaemonMonitorService> _logger = logger;
#pragma warning restore CA1823 // Avoid unused private fields

    private readonly IHostApplicationLifetime _lifeTime = lifeTime;
    private readonly BinOptions _dOptions = dOptions.Value;
    private readonly BinOptions _cliOptions = cliOptions.Value;

    private static Process CreateProcess(string binPath, string arguments) => new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = binPath,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = arguments,
        },
    };

    private enum StartResult
    {
        Success,
        Reused,
        Failed,
    }

    private static StartResult TryStartProcess(Process process)
    {
        try
        {
            return process.Start() ? StartResult.Success : StartResult.Reused;
        }
        catch (Exception e) when (
            e is InvalidOperationException ||
            e is Win32Exception ||
            e is ObjectDisposedException ||
            e is PlatformNotSupportedException)
        {
            return StartResult.Failed;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string daemonPath = Environment.ExpandEnvironmentVariables(_dOptions.BinPath);
        string cliPath = Environment.ExpandEnvironmentVariables(_cliOptions.BinPath);

        var daemonNotFound = string.IsNullOrEmpty(daemonPath) || !File.Exists(daemonPath);
        var cliNotFound = string.IsNullOrEmpty(cliPath) || !File.Exists(cliPath);

        if (daemonNotFound) LogFileNotFound("bitcoind", daemonPath);
        if (cliNotFound) LogFileNotFound("bitcoin-cli", cliPath);

        if (daemonNotFound || cliNotFound) await Task.CompletedTask;

        LogStateServiceMain();

        var daemonArgs = new StringBuilder().AppendJoin(' ', _dOptions.StartArgs).ToString();

        using var daemonProcess = CreateProcess(daemonPath, daemonArgs);

        LogExec(daemonPath, daemonArgs);

        switch (TryStartProcess(daemonProcess))
        {
            case StartResult.Reused:
                LogUsedExistingProcess(daemonPath);
                break;

            case StartResult.Success:
                break;

            case StartResult.Failed:
                LogResultBitcoinDStartFail();

                _lifeTime.StopApplication();

                return;
        }

        LogResultBitcoinDStartSuccess();

        var daemonCompletionTask = daemonProcess.WaitForExitAsync(stoppingToken);
        var cancelledTask = Task.Delay(Timeout.Infinite, stoppingToken);

        LogStateNormal();

        var completedTask = await Task.WhenAny(daemonCompletionTask, cancelledTask);

        if (completedTask == daemonCompletionTask)
        {
            LogAbnormalBitcoinDStopped();

            _lifeTime.StopApplication();

            return;
        }

        LogStateServiceStopping();

        var cliArgs = new StringBuilder().AppendJoin(' ', [.. _cliOptions.StartArgs, "stop"]).ToString();

        using var cliProcess = CreateProcess(cliPath, cliArgs);

        // Don't wait forever
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        cts.Token.Register(LogErrorTimeoutExceeded);

        LogExec(cliPath, cliArgs);

        switch(TryStartProcess(cliProcess)){
            case StartResult.Success:
            case StartResult.Reused:
                LogStateStoppingBitcoinD();

                await cliProcess.WaitForExitAsync(cts.Token);

                break;

            case StartResult.Failed:
                LogErrorCliExec();

                break;
        }

        LogStateWaitingDaemonStopped();

        await daemonProcess.WaitForExitAsync(cts.Token);

        if (!daemonProcess.HasExited)
        {
            LogErrorDaemonRunning();
        }

        LogStateServiceComplete();

        _lifeTime.StopApplication();
    }
}
