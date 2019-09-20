using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Agent.Configuration;
using Loly.Agent.Kafka;
using Loly.Analysers;
using Loly.Kafka.Config;
using Loly.Kafka.Consumer;
using Loly.Kafka.Models;
using Loly.Kafka.Producer;
using Loly.Kafka.Services;
using Loly.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Analysers
{
    public class FileAnalyserHostedService : IHostedService
    {
        private readonly ILogger _log;
        private ConsumerService<Ignore, string> _consumerService;
        private CancellationToken _cancellationtoken;
        private readonly FileAnalyser _analyser;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IProducerService<string, FileInformation> _producerService;
        private readonly IProducerQueue<string, FileInformation> _producerQueue;
        private readonly IConfigProducer _configProducer;
        private readonly LolyAgentFeatureManager _featureManager;

        public FileAnalyserHostedService(FileAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer, LolyAgentFeatureManager featureManager,
            ILogger<FileAnalyserHostedService> logger)
        {
            _featureManager = featureManager;
            _log = logger;
            _analyser = analyser;
            _consumerProvider = consumerProvider;
            _configProducer = configProducer;
            _producerService = new ProducerService<string, FileInformation>(_configProducer, _log);
            _producerQueue = _producerService.Queue;
            InitializeConsumerService();
        }
        
        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.GroupId += "-file_analyser";
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<Ignore, String>(_consumerProvider, consumerConfig, 
                    new List<string>() {"loly-discovered"}, _log);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private void ConsumerServiceOnConsumeResult(object sender, ConsumerConsumeResultHandlerArgs<Ignore, string> args)
        {
            var consumer = args.Consumer;
            var cr = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {cr.TopicPartition});
            _log.LogDebug($"Analysing file for {cr.Value}");
            var result = _analyser.Analyse(cr.Value);
            
            if (result != null)
            {
                _producerQueue.Enqueue(new KafkaMessage<string, FileInformation>()
                {
                    Message = new Message<string, FileInformation>()
                    {
                        Key = cr.Value,
                        Value = result
                    },
                    Topic = "loly-files"
                });
            }
            else
            {
                _log.LogDebug($"Cannot analyse file {cr.Value}");
            }
            
            consumer.Commit(cr);
            consumer.Resume(new List<TopicPartition>() {cr.TopicPartition});
        }

        private void ConsumerServiceOnConsumerError(object sender, ConsumerErrorEventHandlerArgs<Ignore, string> args)
        {
            var consumer = args.Consumer;
            var error = args.Error;
            if (error.IsError)
            {
                try
                {
                    DeInitializeConsumerService();
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error when consuming message.");
                }
                finally
                {
                    _log.LogWarning("Re-Initializing Kafka Consumer");
                    InitializeConsumerService();
                }
            }
        }

        private void DeInitializeConsumerService()
        {
            _consumerService.Stop();
            _consumerService.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_featureManager.IsFileAnalyserEnabled())
            {
                _cancellationtoken = cancellationToken;
                _producerService.Start(_cancellationtoken);
                _consumerService.Consume();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_featureManager.IsFileAnalyserEnabled())
                DeInitializeConsumerService();
            return Task.CompletedTask;
        }
    }
}