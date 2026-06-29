using MatchingEngine.Contracts;
using MatchingEngine.Core;

namespace MatchingEngine.FeedHandler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOrderFeed _feed;
    private readonly IOrderLog _orderlog;

    public Worker(IOrderFeed orderFeed, IOrderLog orderLog, ILogger<Worker> logger)
    {
        _orderlog = orderLog;
        _feed = orderFeed;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        long produced = 0;

        await foreach (var msg in _feed.ReadAsync(stoppingToken))
        {
            OrderAction action;

            switch (msg.Type)
            {
                case FeedEventType.NewOrder:
                    action = OrderAction.Place;
                    break;
                case FeedEventType.Cancel:
                case FeedEventType.Delete:
                    action = OrderAction.Cancel;
                    break;
                default:
                    continue;
            }
            long clientId = OrderId.For(3, msg.OrderId);
            var order = new Order(0, clientId, msg.Symbol, msg.Side, msg.Price, msg.Size, action);
            _orderlog.Append(order);
            produced++;
        }
        _orderlog.Complete();
        _logger.LogInformation("FeedHandler replayed {Count} orders to the orders topic", produced);
    }
}
