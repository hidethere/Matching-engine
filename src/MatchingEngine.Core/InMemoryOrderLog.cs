using System.Threading.Channels;

namespace MatchingEngine.Core
{
    public class InMemoryOrderLog : IOrderLog
    {
        private readonly Channel<Order> _channel = Channel.CreateUnbounded<Order>();

        public bool Append(Order order)
        {
            return _channel.Writer.TryWrite(order);
        }

        public void Complete() => _channel.Writer.Complete();

        public IAsyncEnumerable<Order> ReadAllAsync(CancellationToken ct = default)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }
    }
}
