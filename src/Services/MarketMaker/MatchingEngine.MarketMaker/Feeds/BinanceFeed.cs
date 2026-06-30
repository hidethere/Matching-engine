using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatchingEngine.MarketMaker.Feeds
{
    public class BinanceFeed : IPriceFeed
    {
        private readonly string[] _symbols;

        public BinanceFeed(string[] symbols)
        {
            _symbols = symbols;

        }
        public async IAsyncEnumerable<PriceTick> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var ws = new ClientWebSocket();
            var streams = string.Join("/", _symbols.Select(s => $"{s.ToLower()}@trade"));
            Uri binanceUri = new Uri($"wss://stream.binance.com:9443/stream?streams={streams}");
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

                var root = JsonDocument.Parse(msg.ToArray()).RootElement;
                if (!root.TryGetProperty("data", out var data)
                 || !data.TryGetProperty("s", out var sEl)
                 || !data.TryGetProperty("p", out var pEl))
                    continue;                                   // not a trade-with-data envelope → skip

                string symbol = sEl.GetString()!;
                long price = (long)(decimal.Parse(pEl.GetString()!, CultureInfo.InvariantCulture) * 100);
                yield return new PriceTick(symbol, price);
            }
        }
    }
}
