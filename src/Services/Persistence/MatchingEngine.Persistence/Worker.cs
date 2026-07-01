using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchingEngine.Persistence;

public class Worker : BackgroundService
{
    private readonly ITradeConsumer _trades;
    private readonly IDbContextFactory<TradesDbContext> _dbFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(ITradeConsumer trades, IDbContextFactory<TradesDbContext> dbFactory ,ILogger<Worker> logger)
    {
        _trades = trades;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var trade in _trades.ReadAllAsync(stoppingToken))
        {
            await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

            var alreadyExists = await db.Trades.AnyAsync(t => t.Id == trade.Id, stoppingToken);
            if (alreadyExists)
            {
                _logger.LogWarning("Trade with Id {TradeId} already exists in the database. Skipping.", trade.Id);
                continue;
            }

            db.Trades.Add(new TradeRecord
            {
                Id = trade.Id,
                BuyOrderId = trade.BuyOrderId,
                SellOrderId = trade.SellOrderId,
                Price = trade.Price,
                Quantity = trade.Quantity,
                Symbol = trade.Symbol,
                ExecutedAt = trade.ExecutedAt,
                RecordedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Persisted trade {Id} {Symbol} {Qty}@{Price}",
                trade.Id, trade.Symbol, trade.Quantity, trade.Price);
        }
    }
}
