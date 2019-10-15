using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Analysers;
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
    public class ImageMetadataAnalyserHostedService : IHostedService
    {
        private static ILogger _logger;
        private readonly ImageMetadataAnalyser _analyser;
        private readonly IConfigProducer _configProducer;
        private readonly IConsumerProvider _consumerProvider;
        private readonly LolyAgentFeatureManager _featureManager;
        private readonly IProducerQueue<string, FileMetaData> _producerQueue;
        private readonly IProducerService<string, FileMetaData> _producerService;
        private CancellationToken _cancellationtoken;
        private ConsumerService<string, FileMetaData> _consumerService;

        public ImageMetadataAnalyserHostedService(ImageMetadataAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer, LolyAgentFeatureManager featureManager,
            ILogger<ImageMetadataAnalyserHostedService> logger)
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_featureManager.IsImageMetadataAnalyserEnabled())
            {
                _cancellationtoken = cancellationToken;
                _producerService.Start(_cancellationtoken);
                _consumerService.Consume();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_featureManager.IsImageMetadataAnalyserEnabled())
                DeInitializeConsumerService();
            return Task.CompletedTask;
        }

        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.GroupId += "-image_metadata_analyser";
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<string, FileMetaData>(_consumerProvider, consumerConfig, 
                new List<string>() {Constants.TopicFiles}, _logger);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private void ConsumerServiceOnConsumeResult(object sender, ConsumeResultHandlerArgs<string, FileMetaData> args)
        {
            var consumer = args.Consumer;
            var consumeResult = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {consumeResult.TopicPartition});
            if (consumeResult.Value.MetaData.ContainsKey(Constants.FileMimeType) && consumeResult.Value.MetaData[Constants.FileMimeType].ToLowerInvariant().StartsWith("image/"))
            {
                _logger.LogDebug($"Analysing image metadata for {consumeResult.Value.Path}");

                try
                {
                    var result = _analyser.Analyse(consumeResult.Value.Path);
                    if (result.Count > 0)
                    {
                        var msg = new FileMetaData()
                        {
                            Path = consumeResult.Value.Path,
                            MetaData = result,
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
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to analyse image at {path}", consumeResult.Value.Path);
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
    }
}