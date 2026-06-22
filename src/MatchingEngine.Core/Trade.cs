namespace MatchingEngine.Core;

/// <summary>
/// A trade (a "fill") produced when a buy order and a sell order cross.
///
/// Like <see cref="Order"/>, it is a <c>readonly record struct</c>: an emitted stream of trades
/// is a contiguous run of values, not a list of heap objects. The engine produces these; later
/// layers (market data, persistence) consume them — but never from inside the match loop.
/// </summary>
/// <param name="BuyOrderId">Id of the buy order that participated in this fill.</param>
/// <param name="SellOrderId">Id of the sell order that participated in this fill.</param>
/// <param name="Price">The price the trade executed at — by convention the *resting* (maker)
/// order's price, since it was on the book first.</param>
/// <param name="Quantity">Quantity filled in this trade (the smaller of the two orders' remaining
/// quantities).</param>
public readonly record struct Trade(
    long BuyOrderId,
    long SellOrderId,
    long Price,
    long Quantity);
