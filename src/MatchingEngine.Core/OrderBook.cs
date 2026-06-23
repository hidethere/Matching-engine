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

    private static KeyValuePair<long, LinkedList<Order>> GetBestFront(SortedDictionary<long, LinkedList<Order>> side)
    {
        foreach (var entry in side)
            return entry;
        throw new InvalidOperationException("The book side is empty.");
    }

    public void RemoveBestFront(Side side)
    {
        var book = side == Side.Buy ? _bids : _asks;
        var best = GetBestFront(book);
        var bestPrice = best.Key;
        var bestOrders = best.Value;
        bestOrders.RemoveFirst();
        if (bestOrders.Count == 0)
        {
            book.Remove(bestPrice);  
        }
    }

    public void ReduceBestFront(Side side, long quantity)
    {
        var book = side == Side.Buy ? _bids : _asks;
        var best = GetBestFront(book);
        var bestPrice = best.Key;
        var bestOrders = best.Value;
        var bestOrder = bestOrders.First!.Value;
        var reducedOrder = bestOrder with { Quantity = bestOrder.Quantity - quantity };
        bestOrders.RemoveFirst();

        if (reducedOrder.Quantity > 0)
            bestOrders.AddFirst(reducedOrder);

        if (bestOrders.Count == 0)
            book.Remove(bestPrice);

    }

    public bool TryGetBestBid(out long price)
    {
        if(_bids.Count > 0)
        {
            price = GetBestFront(_bids).Key;
            return true;
        }
        price = 0;
        return false;   
    }

    public bool TryGetBestAsk(out long price)
    {
        if(_asks.Count > 0)
        {
            price = GetBestFront(_asks).Key;
            return true;
        }
        price = 0;
        return false;   
    }

    public bool TryPeekBestBid(out Order order)
    {
        if(_bids.Count > 0)
        {
            order = GetBestFront(_bids).Value.First!.Value;
            return true;
        }
        order = default;
        return false;

    }

    public bool TryPeekBestAsk(out Order order)
    {
        if(_asks.Count > 0)
        {
            order = GetBestFront(_asks).Value.First!.Value;
            return true;
        }
        order = default;
        return false;

    }
}
