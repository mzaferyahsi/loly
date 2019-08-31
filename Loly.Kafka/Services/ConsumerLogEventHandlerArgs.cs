using System;
using Confluent.Kafka;

namespace Loly.Kafka.Services
{
    public class ConsumerLogEventHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey, TValue> Consumer { get; set; }
        public LogMessage Message { get; set; }
        
    }
}