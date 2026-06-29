using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class MatchingEngineTests
{
    [Fact]
    public void Submit_with_no_opposite_orders_just_rests_the_order()
    {
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 1, "AAPL", Side.Buy, 100_00, 5), trades);

        Assert.Empty(trades);
        Assert.True(engine.Book.TryGetBestBid(out var bestBid));
        Assert.Equal(100_00, bestBid);
    }

    [Fact]
    public void Buy_fully_matches_a_resting_sell_at_market_price()
    {
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(0, 9, "AAPL", Side.Buy, 103_00, 5), trades);

        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(5, trades.First().Quantity);
        Assert.False(engine.Book.TryGetBestAsk(out _));
    }

    [Fact]
    public void Buy_larger_than_resting_fills_then_rests_the_remainder()
    {
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(0, 9, "AAPL", Side.Buy, 103_00, 7), trades);

        Assert.Single(trades);
        Assert.Equal(102_00, trades.First().Price);
        Assert.Equal(5, trades.First().Quantity);
        Assert.True(engine.Book.TryGetBestBid(out var bestBid));
        Assert.Equal(103_00, bestBid);
    }

    [Fact]
    public void Buy_smaller_than_resting_partially_fills_the_resting_order()
    {
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 4, "AAPL", Side.Sell, 102_00, 5), trades);
        engine.Submit(new Order(0, 9, "AAPL", Side.Buy, 103_00, 3), trades);

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
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 4, "AAPL", Side.Buy, 100_00, 5), trades);
        engine.Submit(new Order(0, 9, "AAPL", Side.Sell, 99_00, 3), trades);

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
        var engine = new MatchingEngine("AAPL");
        var trades = new List<Trade>();

        engine.Submit(new Order(0, 1, "AAPL", Side.Sell, 101_00, 2), trades);
        engine.Submit(new Order(0, 2, "AAPL", Side.Sell, 102_00, 2), trades);
        engine.Submit(new Order(0, 9, "AAPL", Side.Buy, 103_00, 5), trades);

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
