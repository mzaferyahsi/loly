using System.Collections.Concurrent;
using Loly.Streaming.Models;

namespace Loly.Streaming.Producer
{
    public class ProducerQueue<TKey, TValue> : ConcurrentQueue<StreamMessage<TKey, TValue>>, IProducerQueue<TKey, TValue>
    {
    }
}