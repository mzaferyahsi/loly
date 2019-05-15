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
    }

    public class KafkaProducerHostedService : IKafkaProducerHostedService, IDisposable
    {
        protected readonly ILog _log = LogManager.GetLogger(typeof(KafkaProducerHostedService));
        private readonly IKafkaConfigProducer _configProducer;
        private readonly IKafkaProducerQueue _queue;
        private Timer _timer;
        private Thread _thread;
        private bool _isPublishing = false;

        public KafkaProducerHostedService(IKafkaConfigProducer configProducer, IKafkaProducerQueue queue)
        {
            _configProducer = configProducer;
            _queue = queue;
        }

        private void Schedule()
        {
            if (_timer != null)
                return;

            _thread = new Thread(() =>
            {
                _log.Debug("Scheduling producer.");
                _timer = new Timer((state) =>
                {
                    Publish();
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
                _log.Debug("Producer scheduled.");
                
            });
            _thread.Start();
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
//                this.StopAsync(CancellationToken.None);
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
            if(_isPublishing)
                return;

            var hasMessage = _queue.TryPeek(out var message);
            
            if (!hasMessage)
                return;

            try
            {
                _isPublishing = true;
                using (var p = GetProducer())
                {
                    do
                    {
                        var dequeueResult = _queue.TryDequeue(out message);
                        if (!dequeueResult)
                        {
                            hasMessage = false;
                            break;
                        }

                        try
                        {
                            await p.ProduceAsync(message.Topic,
                                new Message<Null, string>
                                {
                                    Value = message.Message.GetType() != typeof(string)
                                        ? JsonConvert.SerializeObject(message.Message)
                                        : (string) message.Message
                                });

                        }
                        catch (ProduceException<Null, string> e)
                        {
                            _log.Error($"Failed to deliver message: {e.Error.Reason}");
                        }
                    } while (_queue.TryPeek(out message));

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
            
            _log.Debug("Un-scheduling producer.");
            _timer?.Change(Timeout.Infinite, 0);
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }

            _timer = null;
            _thread = null;
            _log.Debug("Producer un-scheduled.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Schedule();
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
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