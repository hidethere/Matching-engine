using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class OrderBookTests
{
    [Fact]
    public void Empty_book_reports_no_best_bid()
    {
        // ARRANGE
        var book = new OrderBook("AAPL");

        // ACT
        var found = book.TryPeekBestBid(out var price);

        // ASSERT
        Assert.False(found);
    }

    [Fact]
    public void Empty_book_reports_no_best_ask()
    {
        // ARRANGE
        var book = new OrderBook("AAPL");

        // ACT
        var found = book.TryPeekBestAsk(out var price);

        // ASSERT
        Assert.False(found);
    }

    [Fact]
    public void Cancel_removes_a_resting_order()
    {
        // ARRANGE
        var book = new OrderBook("BTCUSDT");
        var order = new Order(0, 1, "BTCUSDT", Side.Buy, 100_00, 5);
        book.Rest(order);

        // ACT
        var canceled = book.Cancel(order.ClientOrderId);
        var found = book.TryPeekBestBid(out order);

        // ASSERT
        Assert.True(canceled);
        Assert.False(found);
    }

    [Fact]
    public void Cancel_clientOrderId_that_was_never_rested()
    {
        // ARRANGE
        var book = new OrderBook("BTCUSDT");
        var order = new Order(0, 1, "BTCUSDT", Side.Buy, 100_00, 5);

        // ACT
        var canceled = book.Cancel(order.ClientOrderId);
        var found = book.TryPeekBestBid(out order);

        // ASSERT
        Assert.False(found);
        Assert.False(canceled);

    }

    [Fact]
    public void Cancel_a_partially_rested_order()
    {
        // ARRANGE
        var book = new OrderBook("BTCUSDT");
        var orderBuy = new Order(0, 1, "BTCUSDT", Side.Buy, 100_00, 5);
        var orderSell = new Order(0, 2, "BTCUSDT", Side.Sell, 101_00, 2);

        // ACT
        book.Rest(orderBuy);

        book.ReduceBestFront(orderBuy.Side, orderSell.Quantity);
        var found = book.TryPeekBestBid(out orderBuy);
        var canceled = book.Cancel(orderBuy.ClientOrderId);

        // ASSERT
        Assert.True(found);
        Assert.Equal(3, orderBuy.Quantity);
        Assert.True(canceled);
        Assert.False(book.TryPeekBestBid(out _));

    }
}
