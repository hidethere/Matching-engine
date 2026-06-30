using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;

namespace MatchingEngine.FeedHandler
{
    public enum FeedEventType : byte
    {
        NewOrder = 1,   // add a limit order
        Cancel = 2,   // reduce size (partial cancel)
        Delete = 3,   // remove order (full cancel)
        Execution = 4,   // a resting order was hit
        Other = 0
    }
    
    public readonly record struct FeedMessage(
        long TimeStamp, // normalized (nanoseconds)
        FeedEventType Type,
        long OrderId,
        string Symbol,
        Side Side,
        long Price, // integer ticks
        long Size
        );
}
