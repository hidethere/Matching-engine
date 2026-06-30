using MatchingEngine.Core;
using MatchingEngine.Gateway;
using MatchingEngine.Gateway.Dtos;

namespace MatchingEngine.IntegrationTests
{
    public class OrderPipelineTests
    {
        [Fact]
        public async Task Order_flows_gateway_to_log_to_engine_and_produces_a_trade()
        {
            // ARRANGE
            var orderLog = new InMemoryOrderLog();
            var gateway = new OrderGateway();
            var host = new EngineHost(orderLog);

            // ACT
            var sell = gateway.Accept(new OrderRequest("AAPL", Side.Sell, 102_00, 5, "k1"));
            var buy = gateway.Accept(new OrderRequest("AAPL", Side.Buy, 103_00, 5, "k2"));

            orderLog.Append(sell.Order);
            orderLog.Append(buy.Order);
            orderLog.Complete();

            await host.RunAsync();

            var trades = new List<Trade>();
            await foreach (var t in host.Trades.ReadAllAsync())
            {
                trades.Add(t);

            }

            //ASSERT
            Assert.Single(trades);
            Assert.Equal(102_00, trades[0].Price);
            Assert.Equal(5, trades[0].Quantity);
        }

        [Fact]
        public async Task Router_sends_each_symbol_to_its_own_engine()
        {
            // ARRANGE — two partitions, each a (log + host) pair
            var appleLog = new InMemoryOrderLog();
            var msftLog = new InMemoryOrderLog();
            var appleHost = new EngineHost(appleLog);
            var msftHost = new EngineHost(msftLog);

            var router = new SymbolRouter(new Dictionary<string, IOrderLog>
            {
                ["AAPL"] = appleLog,
                ["MSFT"] = msftLog,
            });

            // ACT — a matching pair for each symbol, through the one router
            router.Route(new Order(0, 1, "AAPL", Side.Sell, 102_00, 5));
            router.Route(new Order(0, 2, "AAPL", Side.Buy, 103_00, 5));
            router.Route(new Order(0, 3, "MSFT", Side.Sell, 50_00, 7));
            router.Route(new Order(0, 4, "MSFT", Side.Buy, 51_00, 7));

            appleLog.Complete();
            msftLog.Complete();
            await appleHost.RunAsync();
            await msftHost.RunAsync();

            var appleTrades = new List<Trade>();
            await foreach (var t in appleHost.Trades.ReadAllAsync()) appleTrades.Add(t);
            var msftTrades = new List<Trade>();
            await foreach (var t in msftHost.Trades.ReadAllAsync()) msftTrades.Add(t);

            // ASSERT — each engine produced only its own symbol's trade
            Assert.Single(appleTrades);
            Assert.Equal(102_00, appleTrades[0].Price);

            Assert.Single(msftTrades);
            Assert.Equal(50_00, msftTrades[0].Price);
        }

        [Fact]
        public async Task Canceled_order_does_not_trade_through_the_pipeline()
        {
            var orderLog = new InMemoryOrderLog();
            var gateway = new OrderGateway();
            var host = new EngineHost(orderLog);

            var sell = gateway.Accept(new OrderRequest("BTCUSDT", Side.Sell, 102_00, 5, "key1"));
            orderLog.Append(sell.Order);

            orderLog.Append(sell.Order with { Action = OrderAction.Cancel });

            var buy = gateway.Accept(new OrderRequest("BTCUSDT", Side.Buy, 103_00, 5, "key2"));
            orderLog.Append(buy.Order);
            orderLog.Complete();

            await host.RunAsync();
            var trades = new List<Trade>();
            await foreach (var t in host.Trades.ReadAllAsync())
                trades.Add(t);

            Assert.Empty(trades);
        }

        [Fact]
        public async Task Same_idempotency_key_appends_only_once()
        {
            var gateway = new OrderGateway();
            var orderLog = new InMemoryOrderLog();

            var first = gateway.Accept(new OrderRequest("BTCUSDT", Side.Buy, 100_00, 5, "Key-1"));
            if (first.IsNew) orderLog.Append(first.Order);

            var second = gateway.Accept(new OrderRequest("BTCUSDT", Side.Buy, 100_00, 5, "Key-1"));
            if (second.IsNew) orderLog.Append(second.Order);

            orderLog.Complete();
            var appended = new List<Order>();
            await foreach (var o in orderLog.ReadAllAsync())
                appended.Add(o);

            Assert.Single(appended);
            Assert.True(first.IsNew);
            Assert.False(second.IsNew);
            Assert.Equal(first.Order.ClientOrderId, second.Order.ClientOrderId);

        }

        [Fact]
        public async Task One_host_matches_each_symbol_in_its_own_book()
        {
            var orderLog = new InMemoryOrderLog();
            var gateway = new OrderGateway();
            var host = new EngineHost(orderLog); 

            orderLog.Append(gateway.Accept(new OrderRequest("BTCUSDT", Side.Sell, 102_00, 5, "a-sell")).Order);
            orderLog.Append(gateway.Accept(new OrderRequest("BTCUSDT", Side.Buy, 103_00, 5, "a-buy")).Order);
            orderLog.Append(gateway.Accept(new OrderRequest("ETHUSDT", Side.Sell, 50_00, 7, "m-sell")).Order);
            orderLog.Append(gateway.Accept(new OrderRequest("ETHUSDT", Side.Buy, 51_00, 7, "m-buy")).Order);
            orderLog.Complete();

            await host.RunAsync();

            var trades = new List<Trade>();
            await foreach (var t in host.Trades.ReadAllAsync()) trades.Add(t);

            Assert.Equal(2, trades.Count);
            Assert.Contains(trades, t => t.Symbol == "BTCUSDT" && t.Price == 102_00);
            Assert.Contains(trades, t => t.Symbol == "ETHUSDT" && t.Price == 50_00);

        }
    }
}
