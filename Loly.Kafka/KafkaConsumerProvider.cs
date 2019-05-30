using System;
using Confluent.Kafka;

namespace Loly.Kafka
{
    public interface IKafkaConsumerProvider
    {
        IConsumer<TKey, TValue> GetConsumer<TKey, TValue>();

        IConsumer<TKey, TValue> GetConsumer<TKey, TValue>(Action<IConsumer<TKey, TValue>, LogMessage> logHandler,
            Action<IConsumer<TKey, TValue>, Error> errorHandler);
    }

    public class KafkaConsumerProvider : IKafkaConsumerProvider
    {
        private readonly IKafkaConfigProducer _configProducer;

        public KafkaConsumerProvider(IKafkaConfigProducer configProducer)
        {
            _configProducer = configProducer;
        }

        private ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>()
        {
            return new ConsumerBuilder<TKey, TValue>(_configProducer.GetConsumerConfig());
        }

        public IConsumer<TKey, TValue> GetConsumer<TKey, TValue>()
        {
            var consumerBuilder = GetConsumerBuilder<TKey, TValue>();
            return consumerBuilder.Build();
        }

        public IConsumer<TKey, TValue> GetConsumer<TKey, TValue>(Action<IConsumer<TKey, TValue>, LogMessage> logHandler,
            Action<IConsumer<TKey, TValue>, Error> errorHandler)
        {
            var consumerBuilder = GetConsumerBuilder<TKey, TValue>();
            if (errorHandler != null)
                consumerBuilder.SetErrorHandler(errorHandler);
            if (logHandler != null)
                consumerBuilder.SetLogHandler(logHandler);

            return consumerBuilder.Build();
        }
    }
}