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
}
