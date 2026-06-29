namespace MatchingEngine.Core;

public enum OrderAction : byte { Place = 0, Cancel =1 }
public readonly record struct Order(
    long Id,
    long ClientOrderId,
    string Symbol,
    Side Side,
    long Price,
    long Quantity,
    OrderAction Action = OrderAction.Place);
