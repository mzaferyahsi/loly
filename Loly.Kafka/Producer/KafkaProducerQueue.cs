using System.Collections.Concurrent;
using Loly.Kafka.Models;

namespace Loly.Kafka.Producer
{
    public class KafkaProducerQueue<TKey, TValue> : ConcurrentQueue<KafkaMessage<TKey, TValue>>, IProducerQueue<TKey, TValue>
    {
    }
}