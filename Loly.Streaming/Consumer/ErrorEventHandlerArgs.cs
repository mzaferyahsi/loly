using System;
using Confluent.Kafka;

namespace Loly.Streaming.Consumer
{
    public class ErrorEventHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey, TValue> Consumer { get; set; }
        public Error Error { get; set; }
    }
}