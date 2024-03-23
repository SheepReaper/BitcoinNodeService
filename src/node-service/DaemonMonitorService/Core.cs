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

    private sealed record StartResult(string Name)
    {
        public static StartResult Success => new(nameof(Success));
        public static StartResult Reused => new(nameof(Reused));
        public static StartResult Failed => new(nameof(Failed));
        public static StartResult FromProcessStart(bool result) => result ? Success : Reused;

        public override string ToString() => Name;
    }

    private static StartResult TryStartProcess(Process process)
    {
        try
        {
            return StartResult.FromProcessStart(process.Start());
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

        var dResult = TryStartProcess(daemonProcess);

        if (dResult == StartResult.Failed)
        {
            LogResultBitcoinDStartFail();

            _lifeTime.StopApplication();

            return;
        }

        if (dResult == StartResult.Reused)
        {
            LogUsedExistingProcess(daemonPath);
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

        if (TryStartProcess(cliProcess) == StartResult.Failed)
        {
            LogErrorCliExec();
        }
        else
        {
            LogStateStoppingBitcoinD();

            await cliProcess.WaitForExitAsync(cts.Token);
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
