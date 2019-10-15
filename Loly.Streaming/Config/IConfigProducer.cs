using Confluent.Kafka;

namespace Loly.Streaming.Config
{
    public interface IConfigProducer
    {
        ConsumerConfig GetConsumerConfig();

        ProducerConfig GetProducerConfig();
    }
}