using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Loly.App.Db.Models
{
    public class DuplicateFileDbModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Hash { get; set; }
        public List<MongoDBRef> Files { get; set; }
    }
}