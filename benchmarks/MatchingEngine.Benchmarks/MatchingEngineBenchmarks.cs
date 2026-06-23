using BenchmarkDotNet.Attributes;
using MatchingEngine.Core;
using Engine = MatchingEngine.Core.MatchingEngine;


[MemoryDiagnoser]
public class MatchingEngineBenchmarks
{
    private Engine _engine = null!;

    [GlobalSetup]
    public void Setup() => _engine = new Engine("AAPL");

    
     [Benchmark]
     public void Submit_a_matching_pair()
     {
        _engine.Submit(new Order(1, "AAPL", Side.Buy, 100_00, 5));
        _engine.Submit(new Order(2, "AAPL", Side.Sell, 100_00, 5));
    }
}
