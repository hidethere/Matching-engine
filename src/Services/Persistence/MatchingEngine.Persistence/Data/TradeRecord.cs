using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingEngine.Persistence.Data
{
    public  class TradeRecord
    {
        public long Id { get; set; }
        public long BuyOrderId { get; set; }
        public long SellOrderId { get; set; }
        public long Price { get; set; }
        public long Quantity { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public DateTimeOffset ExecutedAt { get; set; }
        public DateTimeOffset RecordedAt { get; set; }
    }
}
