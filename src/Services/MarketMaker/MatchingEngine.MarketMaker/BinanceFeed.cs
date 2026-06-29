using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatchingEngine.MarketMaker
{
    public class BinanceFeed : IPriceFeed
    {
        private readonly string _symbol;

        public BinanceFeed(string symbol)
        {
            _symbol = symbol;

        }
        public async IAsyncEnumerable<PriceTick> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var ws = new ClientWebSocket();
            Uri binanceUri = new Uri($"wss://stream.binance.com:9443/ws/{_symbol.ToLower()}@trade");
            await ws.ConnectAsync(binanceUri, ct);
            var buffer = new byte[8 *1024];

            while(!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                using var msg = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(buffer, ct);
                    msg.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close) break;

                using var doc = JsonDocument.Parse(msg.ToArray());
                var root = doc.RootElement;
                string symbol = root.GetProperty("s").GetString()!;
                string priceStr = root.GetProperty("p").GetString()!;

                long price = (long)(decimal.Parse(priceStr, CultureInfo.InvariantCulture) * 100);
                yield return new PriceTick(symbol, price);
            }
        }
    }
}
