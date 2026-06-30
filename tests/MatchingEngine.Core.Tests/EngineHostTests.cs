using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class EngineHostTests
{
    [Fact]
    public async Task Host_processes_orders_and_publishes_the_resulting_trades()
    {
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost(orderLog);
        orderLog.Append(new Order(0, 4, "AAPL", Side.Sell, 102_00, 5));
        orderLog.Append(new Order(0, 5, "AAPL", Side.Buy, 103_00, 5));
        orderLog.Complete();

        await host.RunAsync();

        var trades = new List<Trade>();
        await foreach (var trade in host.Trades.ReadAllAsync())
            trades.Add(trade);

        Assert.Single(trades);
        Assert.Equal(102_00, trades[0].Price);
        Assert.Equal(5, trades[0].Quantity);
    }

    [Fact]
    public async Task Concurrent_producers_all_get_processed_exactly_once()
    {
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost(orderLog);
        orderLog.Append(new Order(0, 1, "AAPL", Side.Sell, 100_00, 1_000_000));

        var consumer = host.RunAsync();

        var producers1 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(0, 1000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers2 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(0, 2000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers3 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(0, 3000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers4 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(0, 4000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        await Task.WhenAll(producers1, producers2, producers3, producers4);
        orderLog.Complete();
        await consumer;

        var trades = new List<Trade>();
        await foreach (var trade in host.Trades.ReadAllAsync())
            trades.Add(trade);

        Assert.Equal(1000, trades.Count);
    }

    [Fact]
    public async Task Canceled_order_does_not_trade()
    {
        // ARRANGE
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost(orderLog);

        // ACT
        orderLog.Append(new Order(0, 1, "BTCUSDT", Side.Sell, 100_00, 5));
        orderLog.Append(new Order(0, 1, "BTCUSDT", Side.Sell, 100_00, 5, OrderAction.Cancel));
        orderLog.Append(new Order(0, 3, "BTCUSDT", Side.Buy, 100_00, 5));
        orderLog.Complete();

        await host.RunAsync();
        var trades = new List<Trade>();

        await foreach (var t in host.Trades.ReadAllAsync())
            trades.Add(t);

        // ASSERT
        Assert.Empty(trades);
    }

    [Fact]
    public async Task Different_symbols_do_not_cross()
    {
        // ARRANGE
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost(orderLog);

        // ACT
        orderLog.Append(new Order(0, 1, "BTCUSDT", Side.Sell, 100_00, 5));
        orderLog.Append(new Order(0, 2, "ETHUSDT", Side.Buy, 100_00, 5));
        orderLog.Complete();
        
        await host.RunAsync();
        var trades = new List<Trade>();

        await foreach (var t in host.Trades.ReadAllAsync())
            trades.Add(t);

        //ASSERT
        Assert.Empty(trades);
    }
}
