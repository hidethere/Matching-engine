using Microsoft.Extensions.Logging.Abstractions;
using MatchingEngine.Core;

namespace MatchingEngine.Engine;

public class Worker : BackgroundService
{
    private readonly EngineHost _host;
    private readonly ITradePublisher _trades;
    private readonly ILogger<Worker> _logger;

    public Worker(EngineHost host, ITradePublisher trades, ILogger<Worker>? logger = null)
    {
        _host = host;
        _trades = trades;
        _logger = logger ?? NullLogger<Worker>.Instance;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = _host.RunAsync(stoppingToken);

        await foreach (var trade in _host.Trades.ReadAllAsync(stoppingToken))
        {
            _trades.Publish(trade);
            _logger.LogInformation("TRADE buy#{Buy} sell#{Sell} {QTY}@{Price}", trade.BuyOrderId, trade.SellOrderId, trade.Quantity, trade.Price);
        }

        _trades.Complete();

        await consumer;
    }
}
