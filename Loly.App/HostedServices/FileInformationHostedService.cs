using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.App.Analysers;
using Loly.App.Db.Models;
using Loly.App.Db.Services;
using Loly.Streaming.Config;
using Loly.Streaming.Consumer;
using Loly.Models;
using Loly.Streaming.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loly.App.HostedServices
{
    public class FileInformationHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private ConsumerService<string, FileMetaData> _consumerService;
        private CancellationToken _cancellationtoken;
        private readonly FilesService _service;
        private readonly IConsumerProvider _consumerProvider;
        private readonly IConfigProducer _configProducer;
        private readonly List<IMetadataAnalyser> _metadataAnalysers;

        public FileInformationHostedService(FilesService service, IConsumerProvider consumerProvider,
            IConfigProducer configProducer,
            ILogger<FileInformationHostedService> logger, DuplicateFileAnalyser duplicateFileAnalyser)
        {
            _logger = logger;
            _service = service;
            _consumerProvider = consumerProvider;
            _configProducer = configProducer;

            _metadataAnalysers = new List<IMetadataAnalyser>()
            {
                duplicateFileAnalyser
            };

            InitializeConsumerService();
        }
        
        private void InitializeConsumerService()
        {
            var consumerConfig = _configProducer.GetConsumerConfig();
            consumerConfig.EnableAutoCommit = false;
            
            _consumerService = new ConsumerService<String, FileMetaData>(_consumerProvider, consumerConfig, 
                    new List<string>() {Constants.TopicFiles}, _logger);

            _consumerService.ConsumerError += ConsumerServiceOnConsumerError;
            _consumerService.ConsumeResult += ConsumerServiceOnConsumeResult;
        }


        private async void ConsumerServiceOnConsumeResult(object sender, ConsumeResultHandlerArgs<string, FileMetaData> args)
        {
            var consumer = args.Consumer;
            var cr = args.ConsumeResult;
            
            consumer.Pause(new List<TopicPartition>() {cr.TopicPartition});

            var file = ToFile(cr.Value);
            
            if (cr.Value.Action == MetadataAction.Delete)
                await DeleteFile(file);
            else
                await ProcessFile(file);

            foreach (var analyser in _metadataAnalysers)
            {
                await analyser.Analyse(cr.Value);
            }

            consumer.Commit(cr);
            consumer.Resume(new List<TopicPartition>() {cr.TopicPartition});
        }


        private static IFile ToFile(FileMetaData fileMetaData)
        {
            var file = new File {Path = fileMetaData.Path};


            if (fileMetaData.MetaData.ContainsKey(Constants.FileMimeType))
                file.MimeType = fileMetaData.MetaData[Constants.FileMimeType];
            
            if (fileMetaData.MetaData.ContainsKey(Constants.FileName))
                file.Name = fileMetaData.MetaData[Constants.FileName];
            
            if (fileMetaData.MetaData.ContainsKey(Constants.FileSize))
                file.Size = long.Parse(fileMetaData.MetaData[Constants.FileSize]);
            
            if (fileMetaData.MetaData.ContainsKey(Constants.FileExtension))
                file.Extension = fileMetaData.MetaData[Constants.FileExtension];
            
            if (fileMetaData.MetaData.ContainsKey(Constants.FileDateCreated))
                file.DateCreated = DateTime.ParseExact(fileMetaData.MetaData[Constants.FileDateCreated], 
                    Constants.DatetimeFormat, CultureInfo.InvariantCulture);
            
            if (fileMetaData.MetaData.ContainsKey(Constants.FileDateModified))
                file.DateModified = DateTime.ParseExact(fileMetaData.MetaData[Constants.FileDateModified], 
                    Constants.DatetimeFormat, CultureInfo.InvariantCulture);
            
            foreach (var x in fileMetaData.MetaData)
            {
                if(x.Key == Constants.FileMimeType && x.Key == Constants.FileName && x.Key == Constants.FileSize &&
                   x.Key == Constants.FileDateCreated && x.Key == Constants.FileDateModified &&
                   x.Key == Constants.FileExtension)
                    continue;
                
                file.MetaData.Add(x.Key, x.Value);
            }

            return file;
        }

        private async Task DeleteFile(IFile file)
        {
            var result = await _service.Get(x => x.Path == file.Path && file.State != FileState.Deleted);
            if (result.Count < 1)
            {
                return;
            }

            var dbFile = result.First();
            dbFile.State = FileState.Deleted;
            await _service.Update(dbFile.Id, dbFile);
        }

        private async Task ProcessFile(IFile file)
        {
            var result = await _service.Get(x => x.Path == file.Path);
            if (result.Count > 0)
            {
                var obj = result.First();
                await UpdateDb(file, obj);
            }
            else
            {
                var dbObj = FileDbModel.FromIFile(file);
                dbObj.Extension = dbObj.Extension.ToLowerInvariant();
                try
                {
                    await _service.Create(dbObj);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to add {file} to database.", file.Path);
                }
            }

        }

        private async Task UpdateDb(IFile information, FileDbModel obj)
        {
            var objHash = obj.GetHash();
            if(!string.IsNullOrEmpty(information.Extension))
                obj.Extension = information.Extension.ToLowerInvariant();
            if(!string.IsNullOrEmpty(information.Name))
                obj.Name = information.Name;
            if(information.Size != 0)
                obj.Size = information.Size;
            if(information.DateCreated != DateTime.MinValue)
                obj.DateCreated = information.DateCreated;
            if(!string.IsNullOrEmpty(information.MimeType))
                obj.MimeType = information.MimeType;
            if(information.DateModified != DateTime.MinValue)
                obj.DateModified = information.DateModified;

            foreach (var (key, infoValue) in information.MetaData)
            {
                if (obj.MetaData.ContainsKey(key))
                {
                    var value = obj.MetaData[key];
                    if (infoValue != value)
                    {
                        obj.MetaData[key] = infoValue;
                    }
                }
                else
                {
                    obj.MetaData.Add(key, infoValue);
                }
            }

            var current = await _service.Get(obj.Id);
            var currentHash = current.GetHash();
            if (currentHash == objHash)
            {
                if(currentHash == obj.GetHash())
                    return;
                
                await _service.Update(obj.Id, obj);
            }
            else
            {
                await UpdateDb(information, current);
            }
        }

        private void ConsumerServiceOnConsumerError(object sender, ErrorEventHandlerArgs<string, FileMetaData> args)
        {
            var consumer = args.Consumer;
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

        private void DeInitializeConsumerService()
        {
            _consumerService.Stop();
            _consumerService.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationtoken = cancellationToken;
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