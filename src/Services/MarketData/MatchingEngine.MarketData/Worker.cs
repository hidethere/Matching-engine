using MatchingEngine.Contracts.Abstractions;

namespace MatchingEngine.MarketData;

public class Worker : BackgroundService
{
    private readonly ITradeConsumer _trades;
    private readonly Dictionary<string, long> _lastPrice = new();
    private readonly ILogger<Worker> _logger;

    public Worker(ITradeConsumer trades,ILogger<Worker> logger)
    {
        _trades = trades;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var trade in _trades.ReadAllAsync(stoppingToken))
        {
            _lastPrice[trade.Symbol] = trade.Price;
            _logger.LogInformation("{Symbol} last {Price}", trade.Symbol, trade.Price);
        }
    }
}
