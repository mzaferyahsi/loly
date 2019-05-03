using Confluent.Kafka;
using log4net;
using Microsoft.Extensions.Options;

namespace Loly.Agent.Kafka
{
    public interface IKafkaConfigProducer
    {
        ConsumerConfig GetConsumerConfig();

        ProducerConfig GetProducerConfig();
    }

    public class KafkaConfigProvider : IKafkaConfigProducer
    {
        KafkaSettings _settings;
        private ILog _log = LogManager.GetLogger(typeof(KafkaConfigProvider));

        public KafkaConfigProvider(IOptions<KafkaSettings> settings)
        {
            _settings = settings.Value;
        }

        public ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig()
            {
                GroupId = _settings.Consumer.GroupId,
                BootstrapServers = _settings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public ProducerConfig GetProducerConfig()
        {
            return new ProducerConfig()
            {
                BootstrapServers = _settings.BootstrapServers
            };
        }
    }
}