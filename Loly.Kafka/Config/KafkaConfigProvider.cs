using System;
using Confluent.Kafka;
using Loly.Kafka.Settings;
using Microsoft.Extensions.Options;

namespace Loly.Kafka.Config
{
    public class ConfigProvider : IConfigProducer
    {
        private readonly KafkaSettings _settings;
//        private ILog _log = LogManager.GetLogger(typeof(KafkaConfigProvider));

        public ConfigProvider(IOptions<KafkaSettings> settings)
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