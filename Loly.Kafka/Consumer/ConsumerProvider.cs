using System;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Loly.Kafka.Config;
using Loly.Kafka.Json;
using Loly.Kafka.Utilities;

namespace Loly.Kafka.Consumer
{
    public class ConsumerProvider : IConsumerProvider
    {
        private readonly IConfigProducer _configProducer;

        public ConsumerProvider(IConfigProducer configProducer)
        {
            _configProducer = configProducer;
        }

        public IConsumer<TKey, TValue> Get<TKey, TValue>()
        {
            var consumerBuilder = GetConsumerBuilder<TKey, TValue>();
            return consumerBuilder.Build();
        }

        public IConsumer<TKey, TValue> Get<TKey, TValue>(ConsumerConfig consumerConfig)
        {
            var consumerBuilder = GetConsumerBuilder<TKey, TValue>(consumerConfig);
            return consumerBuilder.Build();
        }

        public IConsumer<TKey, TValue> Get<TKey, TValue>(Action<IConsumer<TKey, TValue>, LogMessage> logHandler,
            Action<IConsumer<TKey, TValue>, Error> errorHandler)
        {
            var config = _configProducer.GetConsumerConfig();
            return Get(config, logHandler, errorHandler);
        }

        public IConsumer<TKey, TValue> Get<TKey, TValue>(ConsumerConfig consumerConfig, Action<IConsumer<TKey, TValue>, LogMessage> logHandler, Action<IConsumer<TKey, TValue>, Error> errorHandler)
        {
            var consumerBuilder = GetConsumerBuilder<TKey, TValue>(consumerConfig);
            if (errorHandler != null)
                consumerBuilder.SetErrorHandler(errorHandler);
            if (logHandler != null)
                consumerBuilder.SetLogHandler(logHandler);

            if (!Serialization.KafkaCanDeserialize(typeof(TKey)))
            {
                consumerBuilder.SetKeyDeserializer(new JsonDeserializer<TKey>());
            }

            if (!Serialization.KafkaCanDeserialize(typeof(TValue)))
            {
                consumerBuilder.SetValueDeserializer(new JsonDeserializer<TValue>());
            }

            return consumerBuilder.Build();
        }

        private ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>()
        {
            var config = _configProducer.GetConsumerConfig();
            return GetConsumerBuilder<TKey, TValue>(config);
        }

        private ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>(ConsumerConfig consumerConfig)
        {
            return new ConsumerBuilder<TKey, TValue>(consumerConfig);
        }
        
    }
}