using MatchingEngine.Core;
using MatchingEngine.Gateway;
using Xunit;

namespace MatchingEngine.Gateway.Tests;

public class OrderGatewayTests
{
    [Fact]
    public void Valid_request_is_accepted_with_id_1()
    {
        // ARRANGE
        var gateway = new OrderGateway();

        //ACT
        var result = gateway.Accept(new OrderRequest("AAPL", Side.Buy, 100_00, 5, "key-1"));

        // ASSERT
        Assert.True(result.Accepted);
        Assert.Equal(1, result.Order.Id);        
        Assert.Equal("AAPL", result.Order.Symbol);
        Assert.Equal(Side.Buy, result.Order.Side);
        Assert.Equal(100_00, result.Order.Price);
        Assert.Equal(5, result.Order.Quantity);
        Assert.Null(result.RejectReason);
    }

    [Fact]
    public void Ids_are_monotonic_across_requests()
    {
        // ARRANGE
        var gateway = new OrderGateway();

        // ACT
        var first = gateway.Accept(new OrderRequest("AAPL", Side.Buy, 100_00, 5, "k1"));
        var second = gateway.Accept(new OrderRequest("AAPL", Side.Sell, 101_00, 3, "k2"));

        //ASSERT
        Assert.Equal(1, first.Order.Id);
        Assert.Equal(2, second.Order.Id);
    }

    // [Theory] + [InlineData] = the same test body run once per data row (parameterized).
    [Theory]
    [InlineData(0, 5, "AAPL")]      
    [InlineData(100_00, 0, "AAPL")] 
    [InlineData(100_00, 5, "")]     
    [InlineData(100_00, 5, "   ")]  
    public void Invalid_requests_are_rejected(long price, long quantity, string symbol)
    {
        // ARRANGE
        var gateway = new OrderGateway();

        // ACT
        var result = gateway.Accept(new OrderRequest(symbol, Side.Buy, price, quantity, "k"));

        // ASSERT
        Assert.False(result.Accepted);
        Assert.NotNull(result.RejectReason);
    }

    [Fact]
    public void Same_idempotency_key_returns_same_order()
    {
        // ARRANGE
        var gateway = new OrderGateway();
        var req = new OrderRequest("AAPL", Side.Buy, 100_00, 5, "k1");

        // ACT
        var first = gateway.Accept(req);
        var second = gateway.Accept(req);

        // ASSERT
        Assert.Equal(first.Order.Id, second.Order.Id);
    }
}
