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