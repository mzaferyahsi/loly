using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;
using Loly.Agent.Configuration;
using Loly.Agent.Kafka;
using Loly.Analysers;
using Loly.Analysers.Utility;
using Loly.Kafka;
using Loly.Kafka.Config;
using Loly.Kafka.Consumer;
using Loly.Kafka.Models;
using Loly.Kafka.Producer;
using Loly.Kafka.Services;
using Loly.Models;
using Loly.Models.Messages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Analysers
{
    public class FileHashAnalyserHostedService : IHostedService
    {
        private static ILogger _log;
        private CancellationToken _cancellationtoken;
        private ConsumerService<string, FileInformation> _consumerService;
        private readonly FileHashAnalyser _analyser;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IProducerService<string, FileMetaDataMessage> _producerService;
        private readonly IProducerQueue<string, FileMetaDataMessage> _producerQueue;
        private readonly IConfigProducer _configProducer;
        private readonly LolyFeatureManager _featureManager;

        public FileHashAnalyserHostedService(FileHashAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer, LolyFeatureManager featureManager,
            ILogger<FileHashAnalyserHostedService> logger)
        {
            _featureManager = featureManager;
            _log = logger;
            _analyser = analyser;
            _consumerProvider = consumerProvider;
            _configProducer = configProducer;
            _producerService = new ProducerService<string, FileMetaDataMessage>(configProducer, _log);
            _producerQueue = _producerService.Queue;
            InitializeConsumerService();
        }
        
        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.GroupId += "-file_hash_analyser";
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<string, FileInformation>(_consumerProvider, consumerConfig, 
                    new List<string>() {"loly-files"}, _log);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private async void ConsumerServiceOnConsumeResult(object sender, ConsumerConsumeResultHandlerArgs<string, FileInformation> args)
        {
            var consumer = args.Consumer;
            var consumeResult = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {consumeResult.TopicPartition});
            _log.LogDebug($"Analysing file hash for {consumeResult.Value.Path}");
            var result = await _analyser.Analyse(HashMethods.SHA512, consumeResult.Value.Path);
            
            if (!string.IsNullOrEmpty(result))
            {
                var msg = new FileMetaDataMessage()
                {
                    Path = consumeResult.Value.Path,
                    MetaData = new Dictionary<string, string>()
                    {
                        {"hash", result}
                    }
                };
                
                _producerQueue.Enqueue(new KafkaMessage<string, FileMetaDataMessage>()
                {
                    Message = new Message<string, FileMetaDataMessage>()
                    {
                        Key = consumeResult.Value.Path,
                        Value = msg
                    },
                    Topic = "loly-file-metadatas"
                });
            }
            else
            {
                _log.LogDebug($"File has generation is not possible for ${consumeResult.Value.Path}");
            }
            
            consumer.Commit(consumeResult);
            consumer.Resume(new List<TopicPartition>() {consumeResult.TopicPartition});
        }
        
        private void ConsumerServiceOnConsumerError(object sender, ConsumerErrorEventHandlerArgs<string, FileInformation> args)
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
                    _log.LogError(e, "Error on consuming message.");
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
            if (_featureManager.IsFileHashAnalyserEnabled())
            {
                _cancellationtoken = cancellationToken;
                _producerService.Start(_cancellationtoken);
                _consumerService.Consume();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_featureManager.IsFileHashAnalyserEnabled())
                DeInitializeConsumerService();
            return Task.CompletedTask;
        }
    }
}