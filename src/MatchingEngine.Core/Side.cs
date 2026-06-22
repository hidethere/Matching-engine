namespace MatchingEngine.Core;

/// <summary>
/// Which side of the book an order sits on.
/// Backed by <see cref="byte"/> to keep it tiny inside the <see cref="Order"/> struct.
/// </summary>
public enum Side : byte
{
    /// <summary>A bid — willing to buy at or below a price.</summary>
    Buy = 0,

    /// <summary>An ask — willing to sell at or above a price.</summary>
    Sell = 1,
}
