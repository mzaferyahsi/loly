using System;
using Confluent.Kafka;

namespace Loly.Kafka.Services
{
    public class ConsumerException<TKey, TValue> : Exception
    {
        public ConsumerException(IConsumer<TKey, TValue> consumer, string message) : base(message)
        {
            Consumer = consumer;
        }

        public IConsumer<TKey, TValue> Consumer { get; private set; }
    }
}