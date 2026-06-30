using MatchingEngine.Core;
using Xunit;

namespace MatchingEngine.Core.Tests;

public class DomainModelTests
{
    [Fact]
    public void Order_is_immutable_with_produces_a_new_value()
    {
        var original = new Order(0, 1, "AAPL", Side.Buy, 150_00, 10);

        // `with` is non-destructive: it returns a *copy* with one field changed.
        var reduced = original with { Quantity = 4 };

        Assert.Equal(10, original.Quantity); // original untouched
        Assert.Equal(4, reduced.Quantity);
        Assert.NotEqual(original, reduced);
    }

    [Fact]
    public void Trade_carries_the_matched_price_and_quantity()
    {
        var trade = new Trade(BuyOrderId: 1, SellOrderId: 2, Price: 150_00, Quantity: 7, "AAPL");

        Assert.Equal(1, trade.BuyOrderId);
        Assert.Equal(2, trade.SellOrderId);
        Assert.Equal(150_00, trade.Price);
        Assert.Equal(7, trade.Quantity);
    }
}
