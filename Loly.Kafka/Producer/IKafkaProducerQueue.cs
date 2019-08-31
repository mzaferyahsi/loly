using Loly.Kafka.Models;

namespace Loly.Kafka.Producer
{
    public interface IProducerQueue<TKey, TValue>
    {
        void Enqueue(KafkaMessage<TKey, TValue> message);
        bool TryDequeue(out KafkaMessage<TKey, TValue> message);
        bool TryPeek(out KafkaMessage<TKey, TValue> message);
        bool IsEmpty { get; }
    }
}