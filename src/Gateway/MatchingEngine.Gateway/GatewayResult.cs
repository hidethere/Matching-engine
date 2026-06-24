using MatchingEngine.Core;

namespace MatchingEngine.Gateway
{
    public readonly record  struct GatewayResult (
        bool Accepted, Order Order, string? RejectReason);
}
