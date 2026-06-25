using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MatchingEngine.Core;

namespace MatchingEngine.Kafka;

public class KafkaOrderLog : IOrderLog
{
    private readonly string _boostrap;
    private readonly string _topic;
    private readonly IProducer<string, string> _producer;

    public KafkaOrderLog(string boostrapServer, string topic)
    {
        _boostrap = boostrapServer;
        _topic = topic;
        EnsureTopicExists(boostrapServer, topic);
        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig { BootstrapServers = boostrapServer }).Build();
    }

    private static void EnsureTopicExists(string bootstrapServers, string topic)
    {
        using var admin = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

        try
        {
            admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            }).GetAwaiter().GetResult();
        }
        catch (CreateTopicsException ex)
            when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {

            }
        }
    public bool Append(Order order)
    {
        var json = JsonSerializer.Serialize(order);

        // Key by SYMBOL -> all orders for a symbol land on the same partition (ordering preserved)
        _producer.Produce(_topic, new Message<string, string> { Key = order.Symbol, Value = json });
        return true;
    }

    public async IAsyncEnumerable<Order> ReadAllAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        using var consumer = new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = _boostrap,
                GroupId = "matching-engine",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();

        consumer.Subscribe(_topic);

        while (!ct.IsCancellationRequested)
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
            yield return JsonSerializer.Deserialize<Order>(result.Message.Value);
        }
        consumer.Close();
    }

    public void Complete() => _producer.Flush(TimeSpan.FromSeconds(5));

}
