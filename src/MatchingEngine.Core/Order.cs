namespace MatchingEngine.Core;

public readonly record struct Order(
    long Id,
    string Symbol,
    Side Side,
    long Price,
    long Quantity);
