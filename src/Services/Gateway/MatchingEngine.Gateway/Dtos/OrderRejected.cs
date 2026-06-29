using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Gateway.Dtos
{
    public readonly record struct OrderRejected(string Reason);

}
