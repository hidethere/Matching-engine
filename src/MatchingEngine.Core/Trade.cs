namespace MatchingEngine.Core;


public readonly record struct Trade(
    long BuyOrderId,
    long SellOrderId,
    long Price,
    long Quantity);
