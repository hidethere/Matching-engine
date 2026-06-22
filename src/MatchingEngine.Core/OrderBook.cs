using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Core;
public class OrderBook
{
    private readonly SortedDictionary<long, LinkedList<Order>> _bids =
            new(Comparer<long>.Create((a, b) => b.CompareTo(a)));

    private readonly SortedDictionary<long, LinkedList<Order>> _asks = new();
    public string Symbol {get; }
    public OrderBook(string symbol)
    {
        Symbol = symbol;
    }

    public void Rest(Order order)
    {
        var book = order.Side == Side.Buy ? _bids : _asks;
        if(!book.TryGetValue(order.Price, out var ordersEnlisted))
        {
            ordersEnlisted = new LinkedList<Order>();
            book[order.Price] = ordersEnlisted;
        }
        ordersEnlisted.AddLast(order);

    }

    public void RemoveBestFront(Side side)
    {
        if (side == Side.Buy)
        {
            _bids.First().Value.RemoveFirst();
            
            if (_bids.First().Value.Count == 0)
            {
                _bids.Remove(_bids.First().Key);
            }
        } else
        {
            _asks.First().Value.RemoveFirst();
            if (_asks.First().Value.Count == 0)
            {
                _asks.Remove(_asks.First().Key);
            }
        }

    }

    public void ReduceBestFront(Side side, long quantity)
    {
        if (side == Side.Buy)
        {
            var bestBidOrders = _bids.First().Value;
            var bestBidOrder = bestBidOrders.First.Value;
            var reducedOrder = bestBidOrder with { Quantity = bestBidOrder.Quantity - quantity };
            bestBidOrders.RemoveFirst();
            if (reducedOrder.Quantity > 0)
            {
                bestBidOrders.AddFirst(reducedOrder);
            }
            if (bestBidOrders.Count == 0)
            {
                _bids.Remove(_bids.First().Key);
            }
        }
        else
        {
            var bestAskOrders = _asks.First().Value;
            var bestAskOrder = bestAskOrders.First.Value;
            var reducedOrder = bestAskOrder with { Quantity = bestAskOrder.Quantity - quantity };
            bestAskOrders.RemoveFirst();
            if (reducedOrder.Quantity > 0)
            {
                bestAskOrders.AddFirst(reducedOrder);
            }
            if (bestAskOrders.Count == 0)
            {
                _asks.Remove(_asks.First().Key);
            }
        }
    }

    public bool TryGetBestBid(out long price)
    {
        if(_bids.Count > 0)
        {
            price = _bids.First().Key;
            return true;
        }
        price = 0;
        return false;   
    }

    public bool TryGetBestAsk(out long price)
    {
        if(_asks.Count > 0)
        {
            price = _asks.First().Key;
            return true;
        }
        price = 0;
        return false;   
    }

    public bool TryPeekBestBid(out Order order)
    {
        if(_bids.Count > 0)
        {
            order = _bids.First().Value.First.Value;
            return true;
        }
        order = default;
        return false;

    }

    public bool TryPeekBestAsk(out Order order)
    {
        if(_asks.Count > 0)
        {
            order = _asks.First().Value.First.Value;
            return true;
        }
        order = default;
        return false;

    }
}
