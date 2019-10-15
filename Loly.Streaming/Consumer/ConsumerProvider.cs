using System;
using Confluent.Kafka;
using Loly.Streaming.Config;
using Loly.Streaming.Json;
using Loly.Streaming.Utilities;
using Microsoft.Extensions.Logging;

namespace Loly.Streaming.Consumer
{
    public class ConsumerProvider : IConsumerProvider
    {
        private readonly IConfigProducer _configProducer;
        private readonly ILogger<ConsumerProvider> _logger;

        public ConsumerProvider(IConfigProducer configProducer, ILogger<ConsumerProvider> logger)
        {
            _configProducer = configProducer;
            _logger = logger;
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
            try
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
            catch (Exception e)
            {
                _logger.LogError("Unable to create new Kafka Consumer", e);
                throw;
            }
        }

        private ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>()
        {
            var config = _configProducer.GetConsumerConfig();
            return GetConsumerBuilder<TKey, TValue>(config);
        }

        private ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>(ConsumerConfig consumerConfig)
        {
            try
            {
                return new ConsumerBuilder<TKey, TValue>(consumerConfig);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to create new Kafka Consumer Builder", e);
                throw;
            }
        }
        
    }
}