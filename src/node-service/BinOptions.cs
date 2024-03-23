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

internal abstract partial class BinOptionsSetup<TOptions>(string sectionName, IConfiguration configuration) : IConfigureOptions<TOptions> where TOptions : BinOptions
{
    public void Configure(TOptions options) => configuration.GetSection(sectionName).Bind(options);
}

internal sealed class BitcoinCliOptionsSetup(IConfiguration configuration) : BinOptionsSetup<BitcoinCliOptions>("BitcoinCli", configuration) { }
internal sealed class BitcoinDOptionsSetup(IConfiguration configuration) : BinOptionsSetup<BitcoinDOptions>("BitcoinD", configuration) { }