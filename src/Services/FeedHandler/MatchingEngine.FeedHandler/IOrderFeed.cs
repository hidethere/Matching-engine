using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.FeedHandler
{
    public interface IOrderFeed
    {
        IAsyncEnumerable<FeedMessage> ReadAsync(CancellationToken ct = default);
    }
}
