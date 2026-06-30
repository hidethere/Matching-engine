using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.MarketMaker.Tests
{
    public class MarketMakerTests
    {
        [Fact]
        public async Task First_tick_posts_a_bid_and_ask_around_the_price()
        {
            var feed = new ScriptedFeed(new PriceTick("BTCUSDT", 100_00));
            var orderLog = new InMemoryOrderLog();
            var worker = new Worker(feed, orderLog, NullLogger<Worker>.Instance);

            await worker.StartAsync(CancellationToken.None);
            await worker.ExecuteTask!;
            
            orderLog.Complete();
            var orders = new List<Order>();

            await foreach (var o in orderLog.ReadAllAsync())
                orders.Add(o);

            Assert.Equal(2, orders.Count);

            var bid = orders[0];
            Assert.Equal(Side.Buy, bid.Side);
            Assert.Equal(99_95, bid.Price);
            Assert.Equal(OrderAction.Place, bid.Action);

            var ask = orders[1];
            Assert.Equal(Side.Sell, ask.Side);
            Assert.Equal(100_05, ask.Price);
            Assert.Equal(OrderAction.Place, ask.Action);


        }

        [Fact]
        public async Task Next_tick_cancels_the_previous_quotes_then_reposts()
        {
            var feed = new ScriptedFeed(
                new PriceTick("BTCUSDT", 100_00),
                new PriceTick("BTCUSDT", 100_00)
                );
            var orderLog = new InMemoryOrderLog();
            var worker = new Worker(feed, orderLog, NullLogger<Worker>.Instance);

            await worker.StartAsync(CancellationToken.None);
            await worker.ExecuteTask!;

            orderLog.Complete();
            var orders = new List<Order>();

            await foreach (var o in orderLog.ReadAllAsync())
                orders.Add(o);

            Assert.Equal(6, orders.Count);
            Assert.Equal(OrderAction.Cancel, orders[2].Action);
            Assert.Equal(OrderAction.Cancel, orders[3].Action);
            Assert.Equal(orders[0].ClientOrderId, orders[2].ClientOrderId);
            Assert.Equal(orders[1].ClientOrderId, orders[3].ClientOrderId);

        }


        // Fake Feed
        private class ScriptedFeed : IPriceFeed
        {
            private readonly PriceTick[] _ticks;
            public ScriptedFeed(params PriceTick[] ticks) => _ticks = ticks;

            public async IAsyncEnumerable<PriceTick> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
            {
                foreach (var tick in _ticks)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return tick;
                }
                await Task.CompletedTask;
            }
        }
    }
}
