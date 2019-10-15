using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Loly.App.Db.Models;
using Loly.App.Db.Settings;
using Loly.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Loly.App.Db.Services
{
    public class DuplicateFilesService
    {
        private readonly IMongoCollection<DuplicateFileDbModel> _collection;
        private readonly ILogger<DuplicateFilesService> _logger;

        public DuplicateFilesService(ILolyDatabaseSettings databaseSettings, ILogger<DuplicateFilesService> logger)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(Constants.DbName);
            _collection = database.GetCollection<DuplicateFileDbModel>(Constants.DbDuplicateFiles);
            _logger = logger;
            GenerateDbIndexes();
        }

        private void GenerateDbIndexes()
        {
            var hashIndexOptions = new CreateIndexOptions()
            {
                Name = "Hash",
                Unique = true
            };
            var hashIndexKey = Builders<DuplicateFileDbModel>.IndexKeys.Ascending(hashIndexOptions.Name);
            try
            {
                var cim = new CreateIndexModel<DuplicateFileDbModel>(hashIndexKey, hashIndexOptions);
                _collection.Indexes.CreateOne(cim);
            }
            catch (MongoCommandException e)
            {
                _logger.LogInformation(e, "Error when creating the index.");
            }
        }

        public async Task<List<DuplicateFileDbModel>> Get() =>
            await _collection.Find(duplicateFile => true).ToListAsync();

        public async Task<DuplicateFileDbModel> Get(string id) =>
            await _collection.Find(duplicateFile => duplicateFile.Id == id).FirstOrDefaultAsync();

        public async Task<List<DuplicateFileDbModel>> Get(Expression<Func<DuplicateFileDbModel, bool>> expression) =>
            await _collection.Find(expression).ToListAsync();

        public async Task<DuplicateFileDbModel> Create(DuplicateFileDbModel duplicateFile)
        {
            await _collection.InsertOneAsync(duplicateFile);
            return duplicateFile;
        }

        public async Task Update(string id, DuplicateFileDbModel duplicateFileIn) =>
            await _collection.ReplaceOneAsync(duplicateFile => duplicateFile.Id == id, duplicateFileIn);

        public async Task Remove(DuplicateFileDbModel duplicateFileIn) =>
            await _collection.DeleteOneAsync(duplicateFile => duplicateFile.Id == duplicateFileIn.Id);

        public async Task Remove(string id) => 
            await _collection.DeleteOneAsync(duplicateFile => duplicateFile.Id == id);
    }
}