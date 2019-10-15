using System;
using System.Threading;
using System.Threading.Tasks;

namespace Loly.Streaming.Producer
{
    public interface IProducerService<TKey, TValue> : IDisposable
    {
        IProducerQueue<TKey, TValue> Queue { get; set; }
        Task Start(CancellationToken cancellationToken);
        Task Stop(CancellationToken cancellationToken);
    }
}