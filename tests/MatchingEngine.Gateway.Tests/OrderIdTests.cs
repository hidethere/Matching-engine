using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Contracts;

namespace MatchingEngine.Gateway.Tests
{
    public class OrderIdTests
    {
        [Fact]
        public void Same_sequence_from_different_producers_do_not_collide()
        {
            var gatewaId = OrderId.For(producer: 1, sequence: 1);
            var marketMakerId= OrderId.For(producer: 2, sequence: 1);

            Assert.NotEqual(gatewaId, marketMakerId);
        }

        [Fact]
        public void Producer_and_sequence_are_recoverable_from_the_id()
        {
            long id = OrderId.For(producer: 2, sequence: 7);

            Assert.Equal(2L, id >> 56);
            Assert.Equal(7L, id & ((1L << 56)) - 1);
        }
    }
}
