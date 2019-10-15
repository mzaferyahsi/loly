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
    public class FileAnalyserHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private ConsumerService<Ignore, string> _consumerService;
        private CancellationToken _cancellationtoken;
        private readonly FileAnalyser _analyser;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IProducerService<string, FileMetaData> _producerService;
        private readonly IProducerQueue<string, FileMetaData> _producerQueue;
        private readonly IConfigProducer _configProducer;
        private readonly LolyAgentFeatureManager _featureManager;

        public FileAnalyserHostedService(FileAnalyser analyser, IConsumerProvider consumerProvider,
            IConfigProducer configProducer, LolyAgentFeatureManager featureManager,
            ILogger<FileAnalyserHostedService> logger)
        {
            _featureManager = featureManager;
            _logger = logger;
            _analyser = analyser;
            _consumerProvider = consumerProvider;
            _configProducer = configProducer;
            _producerService = new ProducerService<string, FileMetaData>(_configProducer, _logger);
            _producerQueue = _producerService.Queue;
            InitializeConsumerService();
        }
        
        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.GroupId += "-file_analyser";
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<Ignore, String>(_consumerProvider, consumerConfig, 
                    new List<string>() {Constants.TopicDiscovered}, _logger);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private void ConsumerServiceOnConsumeResult(object sender, ConsumeResultHandlerArgs<Ignore, string> args)
        {
            var consumer = args.Consumer;
            var cr = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {cr.TopicPartition});
            _logger.LogDebug($"Analysing file for {cr.Value}");
            var result = _analyser.Analyse(cr.Value);
            
            if (result != null)
            {
                var fileMetadata = ToMetaData(result);
                
                _producerQueue.Enqueue(new StreamMessage<string, FileMetaData>()
                {
                    Message = new Message<string, FileMetaData>()
                    {
                        Key = cr.Value,
                        Value = fileMetadata
                    },
                    Topic = Constants.TopicFiles
                });
            }
            else
            {
                _logger.LogDebug($"Cannot analyse file {cr.Value}");
            }
            
            consumer.Commit(cr);
            consumer.Resume(new List<TopicPartition>() {cr.TopicPartition});
        }

        private void ConsumerServiceOnConsumerError(object sender, ErrorEventHandlerArgs<Ignore, string> args)
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
                    _logger.LogError(e, "Error when consuming message.");
                }
                finally
                {
                    _logger.LogWarning("Re-Initializing Kafka Consumer");
                    InitializeConsumerService();
                }
            }
        }

        private FileMetaData ToMetaData(IFile file)
        {
            var fileMetaData = new FileMetaData {Path = file.Path};

            fileMetaData.MetaData.Add(Constants.FileExtension, file.Extension);
            fileMetaData.MetaData.Add(Constants.FileName, file.Name);
            fileMetaData.MetaData.Add(Constants.FileMimeType, file.MimeType);
            fileMetaData.MetaData.Add(Constants.FileSize, file.Size.ToString());
            fileMetaData.MetaData.Add(Constants.FileDateCreated, file.DateCreated.ToString(Constants.DatetimeFormat));
            fileMetaData.MetaData.Add(Constants.FileDateModified, file.DateModified.ToString(Constants.DatetimeFormat));

            foreach (var metadata in file.MetaData)
            {
                fileMetaData.MetaData.Add(metadata.Key, metadata.Value);
            }

            fileMetaData.Action = MetadataAction.Create;
            
            return fileMetaData;
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