using Confluent.Kafka;

namespace Loly.Kafka.Config
{
    public interface IConfigProducer
    {
        ConsumerConfig GetConsumerConfig();

        ProducerConfig GetProducerConfig();
    }
}