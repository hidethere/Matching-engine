using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.MarketData.Projections;
using MatchingEngine.MarketData.Realtime;

namespace MatchingEngine.MarketData;

public class Worker : BackgroundService
{
    private readonly ITradeConsumer _trades;
    private readonly Ticker _ticker;
    private readonly PriceBroadcaster _bus;
    private readonly ILogger<Worker> _logger;

    public Worker(ITradeConsumer trades, Ticker ticker, PriceBroadcaster bus, ILogger<Worker> logger)
    {
        _trades = trades;
        _ticker = ticker;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var trade in _trades.ReadAllAsync(stoppingToken))
        {
            _ticker.Record(trade.Symbol, trade.Price);
            _bus.Publish(trade.Symbol, trade.Price);
            _logger.LogInformation("{Symbol} last {Price}", trade.Symbol, trade.Price);
        }
    }
}
