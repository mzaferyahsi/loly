using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Hangfire.States;
using log4net;
using Loly.Agent.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Loly.Agent.Kafka
{
    public class KafkaProducerHostedService : IKafkaProducerHostedService, IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(KafkaProducerHostedService));

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
            if(_timer != null)
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
            return new ProducerBuilder<Null, string>(_configProducer.GetProducerConfig()).Build();
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
                    while (_queue.Peek() != null && _isEnabled)
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
            if(_timer != null)
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