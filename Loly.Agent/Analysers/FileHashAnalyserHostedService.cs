using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;
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

namespace Loly.Agent.Analysers
{
    public class FileHashAnalyserHostedService : IHostedService
    {
        private static ILog _log = LogManager.GetLogger(typeof(FileHashAnalyserHostedService));
        private CancellationToken _cancellationtoken;
        private ConsumerService<string, FileInformation> _consumerService;
        private readonly FileHashAnalyser _analyser;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IProducerService<string, FileMetaDataMessage> _producerService;
        private readonly IProducerQueue<string, FileMetaDataMessage> _producerQueue;
        private readonly IConfigProducer _configProducer;

        public FileHashAnalyserHostedService(FileHashAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer)
        {
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
            var cr = args.ConsumeResult;
            HandleMessage(consumer, cr);
        }

        private async void HandleMessage(IConsumer<string, FileInformation> consumer,
            ConsumeResult<string, FileInformation> consumeResult)
        {
            consumer.Pause(new List<TopicPartition>() {consumeResult.TopicPartition});
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
                    _log.Error(e);
                }
                finally
                {
                    _log.Warn("Re-Initializing Kafka Consumer");
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
            _cancellationtoken = cancellationToken;
            _producerService.Start(_cancellationtoken);
            _consumerService.Consume();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DeInitializeConsumerService();
            return Task.CompletedTask;
        }
    }
}