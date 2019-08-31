using System;
using Confluent.Kafka;

namespace Loly.Kafka.Services
{
    public class ConsumerConsumeResultHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey,TValue> Consumer { get; set; }
        public ConsumeResult<TKey, TValue> ConsumeResult { get; set; }
    }
}