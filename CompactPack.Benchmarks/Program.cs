using BenchmarkDotNet.Running;

namespace CompactPack.Benchmarks;

internal class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<PackingBenchmarks>();
    }
}
