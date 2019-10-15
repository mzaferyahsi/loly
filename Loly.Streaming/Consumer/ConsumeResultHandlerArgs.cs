using System;
using Confluent.Kafka;

namespace Loly.Streaming.Consumer
{
    public class ConsumeResultHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey,TValue> Consumer { get; set; }
        public ConsumeResult<TKey, TValue> ConsumeResult { get; set; }
    }
}