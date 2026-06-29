using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class InMemoryOrderLogTests
{
    [Fact]
    public async Task Appended_orders_are_read_back_in_order()
    {
        // ARRANGE
        var orderLog = new InMemoryOrderLog();

        // ACT
        orderLog.Append(new Order(1, 1, "AAPL", Side.Buy, 100_00, 5));
        orderLog.Append(new Order(2, 2, "AAPL", Side.Sell, 101_00, 3));
        orderLog.Complete();

        var read = new List<Order>();
        await foreach (var order in orderLog.ReadAllAsync())
            read.Add(order);

        // ASSERT
        Assert.Equal(2, read.Count);
        Assert.Equal(1, read[0].Id);     // FIFO: first appended = first read
        Assert.Equal(2, read[1].Id);
    }
}
