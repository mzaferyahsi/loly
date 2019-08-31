using System;
using System.Threading;
using System.Threading.Tasks;
using Loly.Kafka.Producer;

namespace Loly.Agent.Kafka
{
    public interface IProducerService<TKey, TValue> : IDisposable
    {
        IProducerQueue<TKey, TValue> Queue { get; set; }
        Task Start(CancellationToken cancellationToken);
        Task Stop(CancellationToken cancellationToken);
    }
}