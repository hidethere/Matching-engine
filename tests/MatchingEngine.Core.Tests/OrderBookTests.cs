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

    [Fact]
    public async Task Host_processes_orders_and_publishes_the_resulting_trades() 
    {
        // ARRANGE
        var host = new EngineHost("AAPL");
        host.Submit(new Order(4, "AAPL", Side.Sell, 102_00, 5));
        host.Submit(new Order(5, "AAPL", Side.Buy, 103_00, 5));
        host.Complete();

        // ACT
        await host.RunAsync();

        var trades = new List<Trade>();
        await foreach(var trade in host.Trades.ReadAllAsync())
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
        var host = new EngineHost("AAPL");
        host.Submit(new Order(1, "AAPL", Side.Sell, 100_00, 1_000_000));

        // ACT
        var consumer = host.RunAsync();

        var producers1 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                host.Submit(new Order(1000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers2 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                host.Submit(new Order(2000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers3 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                host.Submit(new Order(3000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        var producers4 = Task.Run(() => {
            for (int i = 0; i < 250; i++)
                host.Submit(new Order(4000 + i, "AAPL", Side.Buy, 100_00, 1));
        });
        await Task.WhenAll(producers1, producers2, producers3, producers4);
        host.Complete();
        await consumer;

        var trades = new List<Trade>();
        await foreach(var trade in host.Trades.ReadAllAsync())
            trades.Add(trade);

        // ASSERT
        Assert.Equal(1000,trades.Count);
    }
}
