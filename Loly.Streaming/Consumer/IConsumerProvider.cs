using System;
using Confluent.Kafka;

namespace Loly.Streaming.Consumer
{
    public interface IConsumerProvider
    {
        IConsumer<TKey, TValue> Get<TKey, TValue>();
        IConsumer<TKey, TValue> Get<TKey, TValue>(ConsumerConfig consumerConfig);

        IConsumer<TKey, TValue> Get<TKey, TValue>(Action<IConsumer<TKey, TValue>, LogMessage> logHandler,
            Action<IConsumer<TKey, TValue>, Error> errorHandler);

        IConsumer<TKey, TValue> Get<TKey, TValue>(ConsumerConfig consumerConfig,
            Action<IConsumer<TKey, TValue>, LogMessage> logHandler,
            Action<IConsumer<TKey, TValue>, Error> errorHandler);
    }
}