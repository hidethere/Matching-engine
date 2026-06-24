using MatchingEngine.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.Gateway
{
    public class SymbolRouter
    {
        private readonly Dictionary<string, IOrderLog> _routes;
        private readonly ILogger<SymbolRouter> _logger;
        public SymbolRouter(Dictionary<string, IOrderLog> routes, ILogger<SymbolRouter>? logger = null)
        {
            _routes = routes;
            _logger = logger ?? NullLogger<SymbolRouter>.Instance;
        }
        public IOrderLog LogFor(string symbol) => _routes[symbol];

        public bool Route(Order order)
        {
            if (!_routes.TryGetValue(order.Symbol, out var log))
            {
                _logger.LogWarning("No partition  for symbol {Symbol}; order {OrderId} dropped", order.Symbol, order.Id);
                return false;
            }
            log.Append(order);
            return true;
        }

    }
}
