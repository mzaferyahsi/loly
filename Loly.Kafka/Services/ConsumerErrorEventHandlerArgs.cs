using System;
using Confluent.Kafka;

namespace Loly.Kafka.Services
{
    public class ConsumerErrorEventHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey, TValue> Consumer { get; set; }
        public Error Error { get; set; }
    }
}