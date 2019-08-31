using Confluent.Kafka;

namespace Loly.Kafka.Models
{
    public class KafkaMessage<TKey, TValue>
    {
        public string Topic { get; set; }
        public Message<TKey, TValue> Message { get; set; }
    }
}