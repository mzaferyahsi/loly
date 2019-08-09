using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Loly.Kafka
{
    public interface IKafkaConfigProducer
    {
        ConsumerConfig GetConsumerConfig();

        ProducerConfig GetProducerConfig();
    }

    public class KafkaConfigProvider : IKafkaConfigProducer
    {
        private readonly KafkaSettings _settings;
//        private ILog _log = LogManager.GetLogger(typeof(KafkaConfigProvider));

        public KafkaConfigProvider(IOptions<KafkaSettings> settings)
        {
            _settings = settings.Value;
        }

        public ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig
            {
                GroupId = _settings.Consumer.GroupId,
                BootstrapServers = _settings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                MaxPollIntervalMs = (int?) TimeSpan.FromHours(12).TotalMilliseconds
            };
        }

        public ProducerConfig GetProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers
            };
        }
    }
}