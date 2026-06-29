using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;

namespace MatchingEngine.Contracts.Abstractions
{
    public interface ITradeConsumer
    {
        IAsyncEnumerable<Trade> ReadAllAsync(CancellationToken ct = default);
    }
}
