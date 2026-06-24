using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Core
{
    public class MatchingEngine
    {
        public OrderBook Book { get; }

        public MatchingEngine(string symbol)
        {
            Book = new OrderBook(symbol);
        }

        public void Submit(Order incoming, List<Trade> trades)
        {
            long remaining = incoming.Quantity;

            if (incoming.Side == Side.Buy)
            {
                while (remaining > 0 && Book.TryPeekBestAsk(out var resting) && incoming.Price >= resting.Price)
                {
                    long fill = Math.Min(remaining, resting.Quantity);
                    trades.Add(new Trade(incoming.Id, resting.Id, resting.Price, fill));
                    remaining -= fill;

                    if (fill == resting.Quantity)
                        Book.RemoveBestFront(Side.Sell);
                    else
                        Book.ReduceBestFront(Side.Sell, fill); 
                }
            }
            else
            {
                while (remaining > 0 && Book.TryPeekBestBid(out var resting) && incoming.Price <= resting.Price)
                {
                    long fill = Math.Min(remaining, resting.Quantity);
                    trades.Add(new Trade(resting.Id, incoming.Id, resting.Price, fill));
                    remaining -= fill;
                    if (fill == resting.Quantity)
                        Book.RemoveBestFront(Side.Buy);
                    else
                        Book.ReduceBestFront(Side.Buy, fill);
                }
            }

            if(remaining > 0)
            {
                Book.Rest(incoming with { Quantity = remaining });
            }
        }

    }
}
