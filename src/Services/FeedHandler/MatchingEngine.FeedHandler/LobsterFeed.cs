using System.Globalization;
using System.Runtime.CompilerServices;
using Confluent.Kafka;
using MatchingEngine.Core;

namespace MatchingEngine.FeedHandler
{
    public class LobsterFeed : IOrderFeed
    {
        private readonly string _path;
        private readonly string _symbol;

        public LobsterFeed(string path, string symbol)
        {
            _path = path;
            _symbol = symbol;
        }
        public async IAsyncEnumerable<FeedMessage> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var reader = new StreamReader(_path);
            string line; // Time, Type, OrderID, Size, Price, Direction

            while ((line = await reader.ReadLineAsync(ct)) is not null)
            {
                if (line.Length == 0) continue;
                var c = line.Split(',');

                FeedEventType type = int.Parse(c[1]) switch
                {
                    1 => FeedEventType.NewOrder,
                    2 => FeedEventType.Cancel,
                    3 => FeedEventType.Delete,
                    4 or 5 => FeedEventType.Execution,
                    _ => FeedEventType.Other
                };

                yield return new FeedMessage
                (
                    TimeStamp: (long)(double.Parse(c[0], CultureInfo.InvariantCulture) * 1_000_000_000),
                    Type: type,
                    OrderId: long.Parse(c[2]),
                    Symbol: _symbol,
                    Side: int.Parse(c[5]) == 1 ? Side.Buy : Side.Sell,
                    Price: int.Parse(c[4]),
                    Size: int.Parse(c[3])
                );
            }
        }
    }
}
