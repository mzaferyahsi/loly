using Confluent.Kafka;

namespace Loly.Streaming.Models
{
    public class StreamMessage<TKey, TValue>
    {
        public string Topic { get; set; }
        public Message<TKey, TValue> Message { get; set; }
    }
}