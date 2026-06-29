using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Contracts;
using MatchingEngine.Core;
using MatchingEngine.Gateway.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatchingEngine.Gateway
{
    public class OrderGateway
    {
        private long _nextId = 0;
        private readonly ConcurrentDictionary<string, Order> _seen = new();
        private readonly ILogger<OrderGateway> _logger;

        public OrderGateway(ILogger<OrderGateway>? logger = null)
        {
            _logger = logger ?? NullLogger<OrderGateway>.Instance;
        }

        private GatewayResult Reject(string reason, OrderRequest request)
        {
            var response = new GatewayResult()
            {
                Accepted = false,
                Order = default,
                RejectReason = reason
            };
            _logger.LogWarning("Order rejected ({Reason}) for {Symbol}", reason, request.Symbol);
            return response;
        }

        public GatewayResult Accept(OrderRequest request)
        {
            if (request.Price <= 0 || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.Symbol))
                return Reject("Invalid order parameters", request);

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
                return Reject("Idempotency key is required", request);

            bool isNew = false;
            Order order = _seen.GetOrAdd(request.IdempotencyKey, _ =>
            {
                isNew = true;
                long clientId = OrderId.For(1, Interlocked.Increment(ref _nextId));
                return new Order(0, clientId, request.Symbol, request.Side, request.Price, request.Quantity);
            }
            );

            return new GatewayResult()
            {
                Accepted = true,
                Order = order,
                RejectReason = null,
                IsNew = isNew

            };
        }
    }
}
