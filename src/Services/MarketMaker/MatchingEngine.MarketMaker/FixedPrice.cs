using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.MarketMaker
{
    public class FixedPrice : IPriceFeed
    {
        private readonly string _symbol;
        private readonly long _price;

        public FixedPrice(string symbol, long price)
        {
            _symbol = symbol;
            _price = price;
        }
        public async IAsyncEnumerable<PriceTick> ReadAsync(CancellationToken ct = default)
        {
            while(!ct.IsCancellationRequested)
            {
                yield return new PriceTick(_symbol, _price);
                await Task.Delay(1000, ct);
            }
        }
    }
}
