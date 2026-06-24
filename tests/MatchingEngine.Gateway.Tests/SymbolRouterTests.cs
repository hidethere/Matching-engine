using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Core;

namespace MatchingEngine.Gateway.Tests
{
    public class SymbolRouterTests
    {
        [Fact]
        public void Routes_each_symbol_to_its_order_log()
        {
            // ARRANGE
            var appleOrderLog = new InMemoryOrderLog();
            var msfOrderLog = new InMemoryOrderLog();

            // ACT
            var router = new SymbolRouter(new Dictionary<string, IOrderLog>
            {
                ["AAPL"] = appleOrderLog,
                ["MSFT"] = msfOrderLog,
            });

            // ASSERT
            Assert.Same(appleOrderLog, router.LogFor("AAPL"));
            Assert.Same(msfOrderLog, router.LogFor("MSFT"));
        }
    }
}
