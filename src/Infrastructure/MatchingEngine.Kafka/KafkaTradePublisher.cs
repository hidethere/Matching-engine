using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MatchingEngine.Core;

namespace MatchingEngine.Kafka
{
    public class KafkaTradePublisher : ITradePublisher
    {
        private readonly string _topic;
        private readonly IProducer<Null, string> _producer;

        public KafkaTradePublisher(string bootstrapServers, string topic)
        {
            _topic = topic;
            EnsureTopicExists(bootstrapServers, topic);
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        private static void EnsureTopicExists(string bootstrapServers, string topic)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
            
            try
            {
                adminClient.CreateTopicsAsync(new[] 
                    { new TopicSpecification { 
                        Name = topic, 
                        NumPartitions = 1, 
                        ReplicationFactor = 1 
                    } 
                }).GetAwaiter().GetResult();
            }
            catch (CreateTopicsException ex)
                when(ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {

            }
        }
        public void Complete()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
        }

        public void Publish(Trade trade)
        {
            var json = JsonSerializer.Serialize(trade);
            _producer.Produce(_topic, new Message<Null, string> { Value = json });
        }
    }
}
