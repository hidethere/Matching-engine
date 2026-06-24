using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Core
{
    public interface IOrderLog
    {
        bool Append(Order order);
        IAsyncEnumerable<Order> ReadAllAsync(CancellationToken ct = default);
        void Complete();    
    }
}
