using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.FeedHandler
{
    public class BinanceFeed : IOrderFeed
    {
        public IAsyncEnumerable<FeedMessage> ReadAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
