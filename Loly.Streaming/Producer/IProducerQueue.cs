using Loly.Streaming.Models;

namespace Loly.Streaming.Producer
{
    public interface IProducerQueue<TKey, TValue>
    {
        void Enqueue(StreamMessage<TKey, TValue> message);
        bool TryDequeue(out StreamMessage<TKey, TValue> message);
        bool TryPeek(out StreamMessage<TKey, TValue> message);
        bool IsEmpty { get; }
    }
}