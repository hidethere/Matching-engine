using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;

namespace MatchingEngine.Gateway.Dtos
{
    public readonly record struct OrderResponse(Order Order, bool Accepted, string? RejectReason, bool IsNew = false);
}
