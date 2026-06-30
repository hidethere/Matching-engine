using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.MarketMaker
{
    public interface IPriceFeed
    {
        IAsyncEnumerable<PriceTick> ReadAsync(CancellationToken ct = default);

    }
}
