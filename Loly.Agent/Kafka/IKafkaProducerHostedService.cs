using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Loly.Agent.Kafka
{
    public interface IKafkaProducerHostedService : IHostedService
    {
        void AddMessage(KafkaMessage message);
    }
}