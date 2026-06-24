using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;

namespace MatchingEngine.Gateway
{
    public readonly record struct OrderRequest(
    string Symbol, Side Side, long Price, long Quantity, string IdempotencyKey);
}
