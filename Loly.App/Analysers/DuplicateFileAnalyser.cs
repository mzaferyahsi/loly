using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loly.App.Db.Models;
using Loly.App.Db.Services;
using Loly.Models;
using Loly.Streaming.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Loly.App.Analysers
{
    public class DuplicateFileAnalyser : IMetadataAnalyser
    {
        private readonly ILogger<DuplicateFileAnalyser> _logger;
        private readonly FilesService _filesService;
        private readonly DuplicateFilesService _duplicateFilesService;

        public DuplicateFileAnalyser(ILogger<DuplicateFileAnalyser> logger,
            FilesService filesService, DuplicateFilesService duplicateFilesService)
        {
            _logger = logger;
            _filesService = filesService;
            _duplicateFilesService = duplicateFilesService;
        }

        public async Task Analyse(FileMetaData metaDataMessage)
        {
            if (!metaDataMessage.MetaData.ContainsKey(Constants.MetadataKeyHash))
                return;

            var fileQueryResult = await _filesService.Get(x => x.Path == metaDataMessage.Path);
            var file = fileQueryResult.FirstOrDefault();
            if(file == null)
                return;

            var hash = metaDataMessage.MetaData[Constants.MetadataKeyHash];

            var reference = new MongoDBRef(Constants.DbName,Constants.TopicFiles, file.Id);

            var duplicateFilesRecordQueryResult =
                await _duplicateFilesService.Get(x => x.Hash == hash);

            if (duplicateFilesRecordQueryResult.Any())
            {
                var duplicateFiles = duplicateFilesRecordQueryResult.First();
                if (duplicateFiles.Files.Contains(reference))
                    return;
                
                duplicateFiles.Files.Add(reference);
                await _duplicateFilesService.Update(duplicateFiles.Id, duplicateFiles);
            }
            else
            {
                var hashMatchedFilesQueryResult = await _filesService.Get(x =>
                    x.MetaData.ContainsKey(Constants.MetadataKeyHash) && x.MetaData[Constants.MetadataKeyHash] ==
                    hash && x.Path != metaDataMessage.Path);

                if (hashMatchedFilesQueryResult.Any())
                {
                    var newDuplicateFilesRecord = new DuplicateFileDbModel()
                    {
                        Hash = hash,
                        Files = new List<MongoDBRef>()
                        {
                            reference
                        }
                    };

                    foreach (var hashMatchedFile in hashMatchedFilesQueryResult)
                    {
                        newDuplicateFilesRecord.Files.Add(new MongoDBRef(Constants.DbName, 
                            Constants.TopicFiles, hashMatchedFile.Id));
                    }

                    var exists = await _duplicateFilesService.Get(x =>
                        x.Hash == hash);
                    
                    if (exists.Any())
                        await Analyse(metaDataMessage);
                    else
                    {
                        try
                        {
                            await _duplicateFilesService.Create(newDuplicateFilesRecord);
                        }
                        catch (MongoBulkWriteException e)
                        {
                            if (e.Message.ToLowerInvariant().Contains("duplicate"))
                                await Analyse(metaDataMessage);
                            else
                                _logger.LogError(e, "Error when writing document to db with {hash}", hash);
                        }
                    }
                }
            }
        }
    }
}