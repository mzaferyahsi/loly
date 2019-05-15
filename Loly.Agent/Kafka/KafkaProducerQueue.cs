using System.Collections.Concurrent;

namespace Loly.Agent.Kafka
{
    public interface IKafkaProducerQueue
    {
        void Enqueue(KafkaMessage message);
        bool TryDequeue(out KafkaMessage message);
        bool TryPeek(out KafkaMessage message);

    }
    public class KafkaProducerQueue : ConcurrentQueue<KafkaMessage>, IKafkaProducerQueue
    {
        
    }
}