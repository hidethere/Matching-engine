namespace MatchingEngine.Core;


public readonly record struct Trade(
    long BuyOrderId,
    long SellOrderId,
    long Price,
    long Quantity,
    string Symbol,
    long Id = 0,
    DateTimeOffset ExecutedAt = default);
