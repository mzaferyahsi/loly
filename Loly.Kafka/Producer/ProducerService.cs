using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Kafka.Config;
using Loly.Kafka.Json;
using Loly.Kafka.Producer;
using Loly.Kafka.Utilities;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Kafka
{
    public class ProducerService<TKey, TValue> : IProducerService<TKey, TValue>
    {
        private readonly IConfigProducer _configProducer;
        protected readonly ILogger _log;
        private bool _isPublishing;
        private Task _task;
        private Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;

        public ProducerService(IConfigProducer configProducer, ILogger logger)
            : this(configProducer, new KafkaProducerQueue<TKey, TValue>(), logger)
        {
        }

        public ProducerService(IConfigProducer configProducer, IProducerQueue<TKey, TValue> queue, ILogger logger)
        {
            _configProducer = configProducer;
            _log = logger;
            Queue = queue;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public IProducerQueue<TKey, TValue> Queue { get; set; }

        public Task Start(CancellationToken cancellationToken)
        {
            Schedule();
            return Task.CompletedTask;
        }


        public Task Stop(CancellationToken cancellationToken)
        {
            UnSchedule();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            UnSchedule();
            _timer?.Dispose();
        }

        private void Schedule()
        {
            if (_timer != null)
                return;
            
            _timer = new Timer(state =>
            {
                if(_task != null && !_task.IsCompleted)
                    return;
                
                _task = new Task(() =>
                {
//                    _log.LogDebug("Timer ticked..");
                    Publish();
                }, _cancellationTokenSource.Token);
                _task.Start();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
        }

        protected virtual IProducer<TKey, TValue> GetProducer()
        {
            var producerBuilder = new ProducerBuilder<TKey, TValue>(_configProducer.GetProducerConfig());
            producerBuilder.SetLogHandler(LogHandler);
            producerBuilder.SetErrorHandler(ErrorHandler);

            if (!Serialization.KafkaCanSerialize(typeof(TKey)))
            {
                producerBuilder.SetKeySerializer(new JsonSerializer<TKey>());
            }

            if (!Serialization.KafkaCanSerialize(typeof(TValue)))
            {
                producerBuilder.SetValueSerializer(new JsonSerializer<TValue>());
            }
            
            return producerBuilder.Build();
        }

        protected virtual void ErrorHandler(IProducer<TKey, TValue> producer, Error error)
        {
            if (error.IsFatal)
            {
                _log.LogCritical(error.Reason);
                Stop(CancellationToken.None);
            }
            else if (error.IsError)
            {
                _log.LogError(error.Reason);
            }
        }

        protected virtual void LogHandler(IProducer<TKey, TValue> producer, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case SyslogLevel.Info:
                    _log.LogInformation(logMessage.Message);
                    break;
                case SyslogLevel.Alert:
                case SyslogLevel.Warning:
                    _log.LogWarning(logMessage.Message);
                    break;
                case SyslogLevel.Debug:
                    _log.LogDebug(logMessage.Message);
                    break;
                case SyslogLevel.Error:
                    _log.LogError(logMessage.Message);
                    break;
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                    _log.LogCritical(logMessage.Message);
                    break;
                default:
                    _log.LogInformation(logMessage.Message);
                    break;
            }
        }

        protected async void Publish()
        {
            if (Queue.IsEmpty)
                return;

            try
            {
                using (var p = GetProducer())
                {
                    do
                    {
                        var dequeueResult = Queue.TryDequeue(out var message);
                        if (!dequeueResult)
                        {
                            _log.LogDebug("No message found to publish.");
                            break;
                        }

                        try
                        {
                            await p.ProduceAsync(message.Topic, message.Message);
                        }
                        catch (ProduceException<TKey, TValue> e)
                        {
                            _log.LogError($"Failed to deliver message: {e.Error.Reason}");
                        }
                    } while (!Queue.IsEmpty);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when producing kafka message.");
            }
        }

        private void UnSchedule()
        {
            _log.LogDebug("Un-scheduling producer.");
            _timer?.Change(Timeout.Infinite, 0);
            if (_task != null && _task.Status == TaskStatus.Running) _cancellationTokenSource.Cancel();

            _timer = null;
            _task = null;
            _log.LogDebug("Producer un-scheduled.");
        }
    }
}