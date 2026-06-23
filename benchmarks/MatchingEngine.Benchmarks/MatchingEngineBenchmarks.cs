using BenchmarkDotNet.Attributes;
using MatchingEngine.Core;
using Engine = MatchingEngine.Core.MatchingEngine;


[MemoryDiagnoser]
public class MatchingEngineBenchmarks
{
    private Engine _engine = null!;
    private List<Trade> _trades = null!;

    [GlobalSetup]
    public void Setup()
    {
        _engine = new Engine("AAPL");
        _trades = new List<Trade>();
    }
     [Benchmark]
     public void Submit_a_matching_pair()
     {
        _trades.Clear();
        _engine.Submit(new Order(1, "AAPL", Side.Buy, 100_00, 5), _trades);
        _engine.Submit(new Order(2, "AAPL", Side.Sell, 100_00, 5), _trades);
    }
}
