using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class MatchingEngineTests
{
    [Fact]
    public void Submit_with_no_opposite_orders_just_rests_the_order()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(1, "AAPL", Side.Buy, 100_00, 5), trades);

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
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 5), trades);

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
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 7), trades);

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
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 3), trades);

        // ASSERT
        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(3, trades.First().Quantity);
        Assert.True(engine.Book.TryPeekBestAsk(out var resting));
        Assert.Equal(2, resting.Quantity);
        Assert.False(engine.Book.TryGetBestBid(out _));
    }

    [Fact]
    public void Sell_smaller_than_resting_partially_fills_the_resting_bid()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(4, "AAPL", Side.Buy, 100_00, 5), trades);
        engine.Submit(new Order(9, "AAPL", Side.Sell, 99_00, 3), trades);

        // ASSERT
        Assert.Single(trades);
        Assert.Equal(100_00, trades.First().Price);
        Assert.Equal(3, trades.First().Quantity);
        Assert.True(engine.Book.TryPeekBestBid(out var resting));
        Assert.Equal(2, resting.Quantity);
        Assert.False(engine.Book.TryGetBestAsk(out _));
    }

    [Fact]
    public void Buy_sweeps_multiple_ask_levels_then_rests_remainder()
    {
        // ARRANGE
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        // ACT
        engine.Submit(new Order(1, "AAPL", Side.Sell, 101_00, 2), trades);
        engine.Submit(new Order(2, "AAPL", Side.Sell, 102_00, 2), trades);
        engine.Submit(new Order(9, "AAPL", Side.Buy, 103_00, 5), trades);

        // ASSERT
        Assert.Equal(2, trades.Count);
        Assert.Equal(101_00, trades[0].Price);
        Assert.Equal(2, trades[0].Quantity);
        Assert.Equal(102_00, trades[1].Price);
        Assert.Equal(2, trades[1].Quantity);
        Assert.True(engine.Book.TryGetBestBid(out var bid));
        Assert.Equal(103_00, bid);
        Assert.False(engine.Book.TryGetBestAsk(out _));
    }
}
