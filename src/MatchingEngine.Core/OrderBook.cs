using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Core;
public class OrderBook
{
    private readonly SortedDictionary<long, Queue<Order>> _bids =
            new(Comparer<long>.Create((a, b) => b.CompareTo(a)));

    private readonly SortedDictionary<long, Queue<Order>> _asks = new();
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
            ordersEnlisted = new Queue<Order>();
            book[order.Price] = ordersEnlisted;
        }
        ordersEnlisted.Enqueue(order);

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
            order = _bids.First().Value.Peek();
            return true;
        }
        order = default;
        return false;

    }

    public bool TryPeekBestAsk(out Order order)
    {
        if(_asks.Count > 0)
        {
            order = _asks.First().Value.Peek();
            return true;
        }
        order = default;
        return false;

    }
}
