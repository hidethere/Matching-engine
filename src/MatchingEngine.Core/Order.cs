namespace MatchingEngine.Core;

/// <summary>
/// A single order submitted to the engine.
///
/// Design notes (these are the hot-path lessons, not incidental style):
/// <list type="bullet">
///   <item><b>It is a struct</b>, so a queue or array of orders lives in one contiguous
///   block of memory with no per-order heap allocation and no GC pressure.</item>
///   <item><b>It is <c>readonly</c></b>, so once created an order can never be mutated in
///   place — fills produce *new* values. Immutable value types are trivially safe to copy
///   across the single-writer boundary.</item>
///   <item><b>Price and Quantity are <see cref="long"/> integers</b>, never <c>double</c> or
///   <c>decimal</c>. Exchanges quote in whole *ticks* (here: integer minor units, e.g. cents).
///   Integers are exact (no rounding bugs at the matching boundary) and fast in tight loops;
///   <c>decimal</c> is ~10x slower and <c>double</c> can't represent 0.10 exactly.</item>
/// </list>
///
/// The engine trusts that orders are already valid (price &gt; 0, quantity &gt; 0). Validation is
/// the gateway's job in a later layer — it is middleware, never something the hot path re-checks.
/// </summary>
/// <param name="Id">Unique, monotonically assigned order id (assigned upstream by the gateway).</param>
/// <param name="Symbol">Instrument symbol, e.g. "AAPL". A later optimization replaces this with an
/// integer symbol id so the struct holds no heap reference at all.</param>
/// <param name="Side">Buy or Sell.</param>
/// <param name="Price">Limit price in integer minor units (e.g. cents). 150_00 == $150.00.</param>
/// <param name="Quantity">Number of units (lots) to trade.</param>
public readonly record struct Order(
    long Id,
    string Symbol,
    Side Side,
    long Price,
    long Quantity);
