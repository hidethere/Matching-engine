using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.Core
{
    public class EngineHost
    {
        private readonly string _symbol;
        private readonly MatchingEngine _engine;
        private readonly Channel<Order> _inbound = Channel.CreateUnbounded<Order>();
        private readonly Channel<Trade> _outbound = Channel.CreateUnbounded<Trade>();

        private readonly ILogger<EngineHost> _logger;

        public EngineHost(string symbol, ILogger<EngineHost>? logger = null)
        {
            _symbol = symbol;
            _engine = new MatchingEngine(symbol);
            _logger = logger ?? NullLogger<EngineHost>.Instance; // Use a null logger if no logger is provided
        }

        public bool Submit(Order order)
        {
            return _inbound.Writer.TryWrite(order);
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("EngineHost[{Symbol}] consumer started.", _symbol);
            long processed = 0;

            var scratch = new List<Trade>();
            try
            {

                await foreach(var order in _inbound.Reader.ReadAllAsync(ct))
                {
                    scratch.Clear(); // Clear the scratch list before writing trades to the outbound channel
                    _engine.Submit(order, scratch);
                    foreach (var trade in scratch)
                        _outbound.Writer.TryWrite(trade);
                    processed++;
                
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EngineHost[{Symbol}] consumer canceled", _symbol);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "EngineHost[{Symbol}] halted after a fatal error", _symbol);
                throw;
            }
            finally
            {
                _outbound.Writer.Complete();    
                _logger.LogInformation("EngineHost[{Symbol}] stopped after {Processed} orders", _symbol, processed);
            }
        }

        public void Complete() => _inbound.Writer.Complete();

        public ChannelReader<Trade> Trades => _outbound.Reader;
    }
}
