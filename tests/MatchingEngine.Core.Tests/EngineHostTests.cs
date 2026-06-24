using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class EngineHostTests
{
    [Fact]
    public async Task Host_processes_orders_and_publishes_the_resulting_trades()
    {
        // ARRANGE
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost("AAPL", orderLog);
        orderLog.Append(new Order(4, "AAPL", Side.Sell, 102_00, 5));
        orderLog.Append(new Order(5, "AAPL", Side.Buy, 103_00, 5));
        orderLog.Complete();

        // ACT
        await host.RunAsync();

        var trades = new List<Trade>();
        await foreach (var trade in host.Trades.ReadAllAsync())
            trades.Add(trade);

        // ASSERT
        Assert.Single(trades);
        Assert.Equal(102_00, trades[0].Price);
        Assert.Equal(5, trades[0].Quantity);
    }

    [Fact]
    public async Task Concurrent_producers_all_get_processed_exactly_once()
    {
        // ARRANGE
        var orderLog = new InMemoryOrderLog();
        var host = new EngineHost("AAPL", orderLog);
        orderLog.Append(new Order(1, "AAPL", Side.Sell, 100_00, 1_000_000));

        // ACT
        var consumer = host.RunAsync();

        var producers1 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(1000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers2 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(2000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers3 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(3000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers4 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                orderLog.Append(new Order(4000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        await Task.WhenAll(producers1, producers2, producers3, producers4);
        orderLog.Complete();
        await consumer;

        var trades = new List<Trade>();
        await foreach (var trade in host.Trades.ReadAllAsync())
            trades.Add(trade);

        // ASSERT
        Assert.Equal(1000, trades.Count);
    }

}
