using System.Collections.Concurrent;

namespace MatchingEngine.MarketData.Projections
{
    public class Ticker
    {
        private readonly ConcurrentDictionary<string, long> _last = new();
        public void Record(string symbol, long price) => _last[symbol] = price;
        public long? Get(string symbol) => _last.TryGetValue(symbol, out var p) ? p : null;
    }
}
