using BitcoinNodeService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .ConfigureOptions<BitcoinCliOptionsSetup>()
    .ConfigureOptions<BitcoinDOptionsSetup>()
    .AddWindowsService(options => options.ServiceName = "Bitcoin Node Daemon")
    .AddHostedService<DaemonMonitorService>();

await builder.Build().RunAsync();