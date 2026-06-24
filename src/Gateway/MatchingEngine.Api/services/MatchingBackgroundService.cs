
using MatchingEngine.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.Api.services
{
    public class MatchingBackgroundService : BackgroundService
    {
        private readonly EngineHost _host;
        private readonly ILogger<MatchingBackgroundService> _logger;

        public MatchingBackgroundService(EngineHost host, ILogger<MatchingBackgroundService>? logger = null)
        {
            _host = host;
            _logger = logger ?? NullLogger<MatchingBackgroundService>.Instance;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = _host.RunAsync(stoppingToken);

            await foreach(var trade in _host.Trades.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("TRADE buy#{Buy} sell#{Sell} {QTY}@{Price}", trade.BuyOrderId, trade.SellOrderId, trade.Quantity, trade.Price);
            }

            await consumer;
        }
    }
}
