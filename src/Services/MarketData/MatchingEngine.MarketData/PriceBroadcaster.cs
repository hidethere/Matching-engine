using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MatchingEngine.MarketData
{
    public class PriceBroadcaster
    {
        private readonly ConcurrentDictionary<Guid, (string Symbol, Channel<long> Ch)> _subs = new();

        public (Guid id, ChannelReader<long> reader) Subscribe(string symbol)
        {
            var ch = Channel.CreateUnbounded<long>();
            var id = Guid.NewGuid();
            _subs[id] = (symbol, ch);
            return (id, ch.Reader);
        }

        public void Unsubscribe(Guid id) => _subs.TryRemove(id, out _);

        public void Publish(string symbol, long price)
        {
            foreach (var s in _subs.Values)
            {
                if (s.Symbol == symbol)
                    s.Ch.Writer.TryWrite(price);
            }
        }
    }
}
