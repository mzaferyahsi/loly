using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Analysers;
using Loly.Analysers.Utility;
using Loly.Configuration.Agent;
using Loly.Streaming.Config;
using Loly.Streaming.Consumer;
using Loly.Streaming.Models;
using Loly.Streaming.Producer;
using Loly.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Analysers
{
    public class FileHashAnalyserHostedService : IHostedService
    {
        private static ILogger _logger;
        private CancellationToken _cancellationtoken;
        private ConsumerService<string, FileMetaData> _consumerService;
        private readonly FileHashAnalyser _analyser;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IProducerService<string, FileMetaData> _producerService;
        private readonly IProducerQueue<string, FileMetaData> _producerQueue;
        private readonly IConfigProducer _configProducer;
        private readonly LolyAgentFeatureManager _featureManager;

        public FileHashAnalyserHostedService(FileHashAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer, LolyAgentFeatureManager featureManager,
            ILogger<FileHashAnalyserHostedService> logger)
        {
            _featureManager = featureManager;
            _logger = logger;
            _analyser = analyser;
            _consumerProvider = consumerProvider;
            _configProducer = configProducer;
            _producerService = new ProducerService<string, FileMetaData>(configProducer, _logger);
            _producerQueue = _producerService.Queue;
            InitializeConsumerService();
        }
        
        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.GroupId += "-file_hash_analyser";
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<string, FileMetaData>(_consumerProvider, consumerConfig, 
                    new List<string>() {Constants.TopicFiles}, _logger);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private async void ConsumerServiceOnConsumeResult(object sender, ConsumeResultHandlerArgs<string, FileMetaData> args)
        {
            var consumer = args.Consumer;
            var consumeResult = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {consumeResult.TopicPartition});
            _logger.LogDebug($"Analysing file hash for {consumeResult.Value.Path}");

            if (consumeResult.Value.Action != MetadataAction.Delete &&
                consumeResult.Value.MetaData.ContainsKey(Constants.FileSize))
            {
                var result = await _analyser.Analyse(HashMethods.Sha512, consumeResult.Value.Path);
            
                if (!string.IsNullOrEmpty(result))
                {
                    var msg = new FileMetaData()
                    {
                        Path = consumeResult.Value.Path,
                        MetaData = new Dictionary<string, string>()
                        {
                            {Constants.MetadataKeyHash, result}
                        },
                        Action = MetadataAction.Update
                    };
                
                    _producerQueue.Enqueue(new StreamMessage<string, FileMetaData>()
                    {
                        Message = new Message<string, FileMetaData>()
                        {
                            Key = consumeResult.Value.Path,
                            Value = msg
                        },
                        Topic = Constants.TopicFiles
                    });
                }
                else
                {
                    _logger.LogDebug($"File has generation is not possible for ${consumeResult.Value.Path}");
                }
                
            }
            
            consumer.Commit(consumeResult);
            consumer.Resume(new List<TopicPartition>() {consumeResult.TopicPartition});
        }
        
        private void ConsumerServiceOnConsumerError(object sender, ErrorEventHandlerArgs<string, FileMetaData> args)
        {
//            var consumer = args.Consumer;
            var error = args.Error;
            if (error.IsFatal)
            {
                _logger.LogCritical("Error on kafka consumer. {error}", error.Reason);
                try
                {
                    DeInitializeConsumerService();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error on consuming message.");
                }
                finally
                {
                    _logger.LogWarning("Re-Initializing Kafka Consumer");
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