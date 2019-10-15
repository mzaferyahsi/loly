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
    public class FilesService
    {
        private readonly IMongoCollection<FileDbModel> _collection;
        private ILogger<FilesService> _logger;

        public FilesService(ILolyDatabaseSettings databaseSettings, ILogger<FilesService> logger)
        {
            _logger = logger;
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(Constants.DbName);
            _collection = database.GetCollection<FileDbModel>(Constants.DbFiles);
            GenerateDbIndexes();
        }
        
        private void GenerateDbIndexes()
        {
            var pathIndexOptions = new CreateIndexOptions()
            {
                Name = "Path",
                Unique = true
            };
            var pathIndexKey = Builders<FileDbModel>.IndexKeys.Ascending(pathIndexOptions.Name);
            try
            {
                var cim = new CreateIndexModel<FileDbModel>(pathIndexKey, pathIndexOptions);
                _collection.Indexes.CreateOne(cim);
            }
            catch (MongoCommandException e)
            {
                _logger.LogInformation(e, "Error when creating the index.");
            }
        }
        
        public async Task<List<FileDbModel>> Get() =>
            await _collection.Find(fileInformation => true).ToListAsync();

        public async Task<FileDbModel> Get(string id) =>
            await _collection.Find(fileInformation => fileInformation.Id == id).FirstOrDefaultAsync();

        public async Task<List<FileDbModel>> Get(Expression<Func<FileDbModel, bool>> expression) =>
            await _collection.Find(expression).ToListAsync();

        public async Task<FileDbModel> Create(FileDbModel file)
        {
            await _collection.InsertOneAsync(file);
            return file;
        }

        public async Task Update(string id, FileDbModel fileIn) =>
            await _collection.ReplaceOneAsync(fileInformation => fileInformation.Id == id, fileIn);

        public async Task Remove(FileDbModel fileIn) =>
            await _collection.DeleteOneAsync(fileInformation => fileInformation.Id == fileIn.Id);

        public async Task Remove(string id) => 
            await _collection.DeleteOneAsync(fileInformation => fileInformation.Id == id);
    }
}