using BitcoinNodeService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<BitcoinDOptions>(builder.Configuration.GetSection("BitcoinD"))
    .Configure<BitcoinCliOptions>(builder.Configuration.GetSection("BitcoinCli"))
    .AddWindowsService(options => options.ServiceName = "Bitcoin Node Daemon")
    .AddHostedService<DaemonMonitorService>();

await builder.Build().RunAsync();