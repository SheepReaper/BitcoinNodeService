using BitcoinNodeService;

using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<BitcoinDOptions>(builder.Configuration.GetSection("BitcoinD"))
    .Configure<BitcoinCliOptions>(builder.Configuration.GetSection("BitcoinCli"))
    .AddWindowsService(options => options.ServiceName = "Bitcoin Node Daemon")
    .AddHostedService<DaemonMonitorService>();

LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

await builder.Build().RunAsync();