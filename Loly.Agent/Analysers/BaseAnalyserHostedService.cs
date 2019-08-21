using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;
using Loly.Kafka;
using Microsoft.Extensions.Hosting;

namespace Loly.Agent.Analysers
{
    public abstract class BaseAnalyserHostedService<TAnalyser, TKafkaConsumerMessage> : IHostedService, IDisposable
    {
        protected readonly KafkaProducerHostedService _kafkaProducerHostedService;
        protected readonly ILog _log;
        protected TAnalyser _analyser;
        protected CancellationTokenSource _cancellationTokenSource;
        protected IConsumer<Ignore, TKafkaConsumerMessage> _consumer;
        protected Task _consumeTask;
        protected IKafkaConsumerProvider _kafkaConsumerProvider;
        protected IKafkaProducerQueue _producerQueue;
        private bool _enabled;

        public BaseAnalyserHostedService(IKafkaConfigProducer configProducer,
            IKafkaConsumerProvider consumerProvider, TAnalyser analyser)
        {
            _log = LogManager.GetLogger(GetType());
            _analyser = analyser;
            _kafkaConsumerProvider = consumerProvider;
            _kafkaProducerHostedService = new KafkaProducerHostedService(configProducer);
            _producerQueue = _kafkaProducerHostedService.Queue;
            _cancellationTokenSource = new CancellationTokenSource();
            _kafkaProducerHostedService.StartAsync(_cancellationTokenSource.Token);
        }

        public abstract string ConsumerTopic { get; }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public virtual void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _kafkaProducerHostedService.StopAsync(_cancellationTokenSource.Token);
            _kafkaProducerHostedService?.Dispose();
            _cancellationTokenSource?.Dispose();
            _consumer?.Dispose();
            _consumeTask?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitialiseConsumer();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DeinitialiseConsumer();
            return Task.CompletedTask;
        }

        protected void ErrorHandler(IConsumer<Ignore, TKafkaConsumerMessage> consumer, Error error)
        {
            if (error.IsFatal)
            {
                _log.Fatal(error.Reason);
                StopAsync(_cancellationTokenSource.Token);
            }
            else if (error.IsError)
            {
                _log.Error(error.Reason);
                StopAsync(_cancellationTokenSource.Token);
            }

            throw new KafkaException(error.Code);
        }

        protected void LogHandler(IConsumer<Ignore, TKafkaConsumerMessage> consumer, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case SyslogLevel.Info:
                    _log.Info(logMessage.Message);
                    break;
                case SyslogLevel.Alert:
                case SyslogLevel.Warning:
                    _log.Warn(logMessage.Message);
                    break;
                case SyslogLevel.Debug:
                    _log.Debug(logMessage.Message);
                    break;
                case SyslogLevel.Error:
                    _log.Error(logMessage.Message);
                    break;
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                    _log.Fatal(logMessage.Message);
                    break;
                default:
                    _log.Info(logMessage.Message);
                    break;
            }
        }

        public void StartConsumer()
        {
            _log.Debug("Initializing kafka consumer for {this.GetType().Name}.");
            if (_consumer != null)
            {
                _log.Warn("There's already an initialized kafka consumer for {this.GetType().Name}.");
                DeinitialiseConsumer();
            }

            _consumeTask = Task.Run(async () =>
            {
                if (_consumer == null)
                    throw new NullReferenceException("Consumer is not initialized.");

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var cr = _consumer.Consume(CancellationToken.None);
                    _consumer.Pause(new List<TopicPartition> {cr.TopicPartition});
                    await Consume(cr);
                    _consumer.Resume(new List<TopicPartition> {cr.TopicPartition});
                }
            }, _cancellationTokenSource.Token);

            _log.Debug($"Initialized kafka consumer for {GetType().Name}.");
        }

        public virtual void DeinitialiseConsumer()
        {
            _log.Debug("De-initializing kafka consumer for file analyser.");
            try
            {
                if (_consumer != null)
                {
                    _consumer.Close();
                    _consumer = null;
                }
            }
            catch (KafkaException e)
            {
                _log.Warn(e);
                _consumer = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (_consumeTask != null)
            {
                _cancellationTokenSource.Cancel();
                _consumeTask = null;
            }
        }

        protected void InitialiseConsumer()
        {
            if (!Enabled)
                return;

            _consumer = _kafkaConsumerProvider.GetConsumer<Ignore, TKafkaConsumerMessage>(LogHandler, ErrorHandler);
            _consumer.Subscribe(ConsumerTopic);
            StartConsumer();
        }

        public abstract Task<bool> Consume(ConsumeResult<Ignore, TKafkaConsumerMessage> message);

        protected void ProduceMessage(string topic, object message)
        {
            var kafkaMessage = new KafkaMessage
            {
                Topic = topic,
                Message = message
            };

            _producerQueue.Enqueue(kafkaMessage);
        }
    }
}