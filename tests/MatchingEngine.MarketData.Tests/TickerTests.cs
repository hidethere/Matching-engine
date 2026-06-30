using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.MarketData.Projections;

namespace MatchingEngine.MarketData.Tests
{
    public class TickerTests
    {
        [Fact]
        public void Get_returns_the_last_recorded_price()
        {
            var ticker = new Ticker();
            ticker.Record("BTCUSDT", 100_00);
            var price = ticker.Get("BTCUSDT");

            Assert.Equal(100_00, price);
        }

        [Fact]
        public void Get_return_null_for_an_unknown_symbol()
        {
            var ticker = new Ticker();
            var price = ticker.Get("BTCUSDT");

            Assert.Null(price);
        }
    }
}
