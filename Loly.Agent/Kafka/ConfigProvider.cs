using Confluent.Kafka;
using log4net;
using Microsoft.Extensions.Options;

namespace Loly.Agent.Kafka
{
    public class ConfigProvider : IKafkaConfigProducer
    {
        KafkaSettings _settings;
        private ILog _log = LogManager.GetLogger(typeof(ConfigProvider));
        
        public ConfigProvider(IOptions<KafkaSettings> settings)
        {
            _settings = settings.Value;
        }

        public ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig()
            {
                GroupId = _settings.Consumer.GroupId,
                BootstrapServers = _settings.BootstrapServers
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