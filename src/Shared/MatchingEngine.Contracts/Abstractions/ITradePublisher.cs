using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Core
{
    public interface ITradePublisher
    {
        void Publish(Trade trade);
        void Complete();
    }
}
