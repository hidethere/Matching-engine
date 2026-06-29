using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.MarketMaker
{
    public readonly record struct PriceTick (
            string Symbol,
            long Price
        );
}
