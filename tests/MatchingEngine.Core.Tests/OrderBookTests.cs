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
    public void Submit_with_no_opposite_orders_just_rests_the_order()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        
        // ACT
        var trades = engine.Submit(
            new Order(1, "AAPL", Side.Buy, 100_00, 5));

        // ASSERT
        Assert.Empty(trades);
        Assert.True(engine.Book.TryGetBestBid(out var bestBid));
        Assert.Equal(100_00, bestBid);
    }

    [Fact]
    public void Buy_fully_matches_a_resting_sell_at_market_price()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        
        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5));
        var trades = engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 5));

        // ASSERT
        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(5, trades.First().Quantity);
        Assert.False(engine.Book.TryGetBestAsk(out _));
    }

    [Fact] 
    public void Buy_larger_than_resting_fills_then_rests_the_remainder()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        
        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5));
        var trades = engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 7));
        // ASSERT
        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(5, trades.First().Quantity);
        Assert.True(engine.Book.TryGetBestBid(out var bestBid));
        Assert.Equal(103_00, bestBid);
    }

    [Fact]
    public void Buy_smaller_than_resting_partially_fills_the_resting_order()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        
        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5));
        var trades = engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 3));
        // ASSERT
        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(3, trades.First().Quantity);
        Assert.True(engine.Book.TryPeekBestAsk(out var resting));
        Assert.Equal(2, resting.Quantity);
        Assert.False(engine.Book.TryGetBestBid(out _));
    }

}
