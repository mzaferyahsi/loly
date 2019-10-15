using System;
using Confluent.Kafka;

namespace Loly.Streaming.Consumer
{
    public class LogEventHandlerArgs<TKey, TValue> : EventArgs
    {
        public IConsumer<TKey, TValue> Consumer { get; set; }
        public LogMessage Message { get; set; }
        
    }
}