using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Hangfire.States;
using log4net;
using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Error = Confluent.Kafka.Error;

namespace Loly.Agent.Kafka
{
    public interface IKafkaProducerHostedService : IHostedService
    {
        void AddMessage(KafkaMessage message);
    }

    public class KafkaProducerHostedService : IKafkaProducerHostedService, IDisposable
    {
        protected readonly ILog _log = LogManager.GetLogger(typeof(KafkaProducerHostedService));

        private Queue<KafkaMessage> _queue = new Queue<KafkaMessage>();
        private IKafkaConfigProducer _configProducer;
        private bool _isEnabled = false;
        private bool _isPublishing = false;
        private Timer _timer;

        public KafkaProducerHostedService(IKafkaConfigProducer configProducer)
        {
            _configProducer = configProducer;
        }

        private void Schedule()
        {
            if (_timer != null)
                UnSchedule();

            _log.Info("Scheduling producer.");
            _timer = new Timer(Publish, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            _log.Info("Producer scheduled.");
        }

        public void AddMessage(KafkaMessage message)
        {
            _queue.Enqueue(message);
        }

        private void Publish(object state)
        {
            Publish();
        }

        protected virtual IProducer<Null, string> GetProducer()
        {
            var producerBuilder = new ProducerBuilder<Null, string>(_configProducer.GetProducerConfig());
            producerBuilder.SetLogHandler(LogHandler);
            producerBuilder.SetErrorHandler(ErrorHandler);
            return producerBuilder.Build();
        }

        protected virtual void ErrorHandler(IProducer<Null, string> producer, Error error)
        {
            if (error.IsFatal)
            {
                _log.Fatal(error.Reason);
                this.StopAsync(CancellationToken.None);
            }
            else if (error.IsError)
            {
                _log.Error(error.Reason);
                this.StopAsync(CancellationToken.None);
            }
        }

        protected virtual void LogHandler(IProducer<Null, string> producer, LogMessage logMessage)
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
            if (_queue.Count < 1 || _isPublishing)
                return;

            try
            {
                _isPublishing = true;

                using (var p = GetProducer())
                {
                    while (_queue.Count > 0 && _isEnabled)
                    {
                        var message = _queue.Dequeue();

                        try
                        {
                            var dr = await p.ProduceAsync(message.Topic,
                                new Message<Null, string> {Value = JsonConvert.SerializeObject(message.Message)});
                            _log.Debug($"Delivered {dr.Value} to {dr.TopicPartitionOffset}");
                        }
                        catch (ProduceException<Null, string> e)
                        {
                            _log.Error($"Failed to deliver message: {e.Error.Reason}");
                            //_queue.Enqueue(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            finally
            {
                _isPublishing = false;
            }
        }

        private void UnSchedule()
        {
            _log.Info("Un-scheduling producer.");
            if (_timer != null)
                _timer.Change(Timeout.Infinite, 0);

            _timer = null;
            _log.Info("Producer un-scheduled.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _isEnabled = true;
            Schedule();
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isEnabled = false;
            UnSchedule();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            UnSchedule();
            _timer?.Dispose();
        }
    }
}