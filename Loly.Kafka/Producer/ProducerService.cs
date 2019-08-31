using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using log4net;
using Loly.Kafka;
using Loly.Kafka.Config;
using Loly.Kafka.Json;
using Loly.Kafka.Producer;
using Loly.Kafka.Utilities;
using Newtonsoft.Json;

namespace Loly.Agent.Kafka
{
    public class ProducerService<TKey, TValue> : IProducerService<TKey, TValue>
    {
        private readonly IConfigProducer _configProducer;
        protected readonly ILog _log;
        private bool _isPublishing;
        private Task _task;
        private Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;

        public ProducerService(IConfigProducer configProducer, ILog logger)
            : this(configProducer, new KafkaProducerQueue<TKey, TValue>(), logger)
        {
        }

        public ProducerService(IConfigProducer configProducer, IProducerQueue<TKey, TValue> queue, ILog logger)
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
                    _log.Debug("Timer ticked..");
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
                _log.Fatal(error.Reason);
                Stop(CancellationToken.None);
            }
            else if (error.IsError)
            {
                _log.Error(error.Reason);
            }
        }

        protected virtual void LogHandler(IProducer<TKey, TValue> producer, LogMessage logMessage)
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
                            _log.Debug("No message found to publish.");
                            break;
                        }

                        try
                        {
                            await p.ProduceAsync(message.Topic, message.Message);
                        }
                        catch (ProduceException<TKey, TValue> e)
                        {
                            _log.Error($"Failed to deliver message: {e.Error.Reason}");
                        }
                    } while (!Queue.IsEmpty);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private void UnSchedule()
        {
            _log.Debug("Un-scheduling producer.");
            _timer?.Change(Timeout.Infinite, 0);
            if (_task != null && _task.Status == TaskStatus.Running) _cancellationTokenSource.Cancel();

            _timer = null;
            _task = null;
            _log.Debug("Producer un-scheduled.");
        }
    }
}