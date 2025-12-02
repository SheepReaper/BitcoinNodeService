using Microsoft.Extensions.Options;

namespace BitcoinNodeService;

internal abstract class BinOptions
{
    public abstract string BinPath { get; set; }
    public string[] StartArgs { get; set; } = [];
}

internal sealed class BitcoinDOptions : BinOptions
{
    public override string BinPath { get; set; } = "%ProgramFiles%\\Bitcoin\\daemon\\bitcoind.exe";
}

internal sealed class BitcoinCliOptions : BinOptions
{
    public override string BinPath { get; set; } = "%ProgramFiles%\\Bitcoin\\daemon\\bitcoin-cli.exe";
}

internal sealed class BitcoinCliOptionsSetup(IConfiguration configuration) : IConfigureOptions<BitcoinCliOptions>
{
    public void Configure(BitcoinCliOptions options) => configuration.GetSection("BitcoinCli").Bind(options);
}


internal sealed class BitcoinDOptionsSetup(IConfiguration configuration) : IConfigureOptions<BitcoinDOptions>
{
    public void Configure(BitcoinDOptions options) => configuration.GetSection("BitcoinD").Bind(options);
}