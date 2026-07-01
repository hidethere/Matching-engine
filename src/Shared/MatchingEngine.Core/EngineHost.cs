using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.Core
{
    public class EngineHost
    {
        private long _tradeSeq;
        private readonly Dictionary<string, MatchingEngine> _engines;
        private readonly Channel<Trade> _outbound = Channel.CreateUnbounded<Trade>();
        private readonly IOrderLog _orderLog;

        private readonly ILogger<EngineHost> _logger;

        public EngineHost(IOrderLog orderlog, ILogger<EngineHost>? logger = null)
        {
            _engines = new Dictionary<string, MatchingEngine>();
            _orderLog = orderlog;
            _logger = logger ?? NullLogger<EngineHost>.Instance; // Use a null logger if no logger is provided

        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("EngineHost consumer started.");
            long processed = 0;


            var scratch = new List<Trade>();
            try
            {

                await foreach(var order in _orderLog.ReadAllAsync(ct))
                {
                    if (!_engines.TryGetValue(order.Symbol, out var engine))
                        _engines.Add(order.Symbol, engine = new MatchingEngine(order.Symbol));
                        
                    if (order.Action == OrderAction.Cancel)
                    {
                            engine.Cancel(order.ClientOrderId);
                    }
                    else
                    {
                        var stamped = order with { Id = ++_tradeSeq };
                        scratch.Clear(); // Clear the scratch list before writing trades to the outbound channel
                        engine.Submit(stamped, scratch);
                        foreach (var trade in scratch)
                            _outbound.Writer.TryWrite(trade with { Id = ++_tradeSeq, ExecutedAt = DateTimeOffset.UtcNow });
                    }
                    processed++;
                
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EngineHost consumer canceled");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "EngineHost halted after a fatal error");
                throw;
            }
            finally
            {
                _outbound.Writer.Complete();    
                _logger.LogInformation("EngineHost stopped after {Processed} orders across {Symbols} symbols", processed, _engines.Count);
            }
        }
        public ChannelReader<Trade> Trades => _outbound.Reader;
    }
}
