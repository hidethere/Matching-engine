using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Contracts
{
    public static class OrderId
    {
        public static long For(byte producer, long sequence) => ((long)producer << 56 | sequence);
    }
}
