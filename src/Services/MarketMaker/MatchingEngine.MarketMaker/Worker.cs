using System.IO.Pipelines;
using MatchingEngine.Contracts;
using MatchingEngine.Core;

namespace MatchingEngine.MarketMaker;

public class Worker : BackgroundService
{
    private readonly IPriceFeed _priceFeed;
    private readonly IOrderLog _orderLog;
    private readonly ILogger<Worker> _logger;
    private long _nextId;
    private readonly Dictionary<string, (long Bid, long Ask)> _activeQuotes = new();


    public Worker(IPriceFeed priceFeed, IOrderLog orderLog ,ILogger<Worker> logger)
    {
        _priceFeed = priceFeed;
        _orderLog = orderLog;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var tick in _priceFeed.ReadAsync(stoppingToken))
        {
            Order bidOrder;
            Order askOrder;
            if (_activeQuotes.TryGetValue(tick.Symbol, out var activeOrders))
            {
                bidOrder = new Order(
                    0,
                    activeOrders.Bid,
                    tick.Symbol,
                    Side.Buy,
                    tick.Price,
                    1,
                    OrderAction.Cancel
                    );

                askOrder = new Order(
                    0,
                    activeOrders.Ask,
                    tick.Symbol,
                    Side.Sell,
                    tick.Price,
                    1,
                    OrderAction.Cancel
                    );

                _orderLog.Append(bidOrder);
                _orderLog.Append(askOrder);
            }

            const long SpreadBps = 10; // 0.10%
            long half = tick.Price * SpreadBps / 10000 / 2;
            if (half < 1) half = 1; // guards symbols from rounding to 0
            long bid = tick.Price - half;
            long ask = tick.Price + half;

            var bidId = OrderId.For(2, Interlocked.Increment(ref _nextId));
            var askId = OrderId.For(2, Interlocked.Increment(ref _nextId));

            bidOrder = new Order(
                    0,
                    bidId,
                    tick.Symbol,
                    Side.Buy,
                    bid,
                    1
                    );

            askOrder = new Order(
                0,
                askId,
                tick.Symbol,
                Side.Sell,
                ask,
                1
                );

            _orderLog.Append(bidOrder);
            _orderLog.Append(askOrder);
            _activeQuotes[tick.Symbol] = (bidId, askId);
        }
    }
}
