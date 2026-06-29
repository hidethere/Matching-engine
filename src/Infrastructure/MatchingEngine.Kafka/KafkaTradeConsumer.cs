using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Core;
using static Confluent.Kafka.ConfigPropertyNames;

namespace MatchingEngine.Kafka
{
    public class KafkaTradeConsumer : ITradeConsumer
    {
        private readonly string _boostrap;
        private readonly string _topic;

        public KafkaTradeConsumer(string boostrap, string topic)
        {
            _boostrap = boostrap;
            _topic = topic;
        }

        public async IAsyncEnumerable<Trade> ReadAllAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var consumer = new ConsumerBuilder<string, string>(
                new ConsumerConfig
                {
                    BootstrapServers = _boostrap,
                    GroupId = "market-data",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }
            ).Build();
            consumer.Subscribe(_topic);

            while(!ct.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = await Task.Run(() => consumer.Consume(ct));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException)
                {
                    await Task.Delay(500, ct);
                    continue;
                }
                if (result?.Message?.Value is null) continue;
                yield return JsonSerializer.Deserialize<Trade>(result.Message.Value);

            }
        }
    }
}
