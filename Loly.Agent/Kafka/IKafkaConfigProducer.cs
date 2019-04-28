using Confluent.Kafka;

namespace Loly.Agent.Kafka
{
    public interface IKafkaConfigProducer
    {
        ConsumerConfig GetConsumerConfig();

        ProducerConfig GetProducerConfig();
    }
}