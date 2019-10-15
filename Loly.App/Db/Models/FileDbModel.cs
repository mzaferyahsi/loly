using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Loly.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Loly.App.Db.Models
{
    public class FileDbModel : IFile
    {
        public FileDbModel()
        {
            this.MetaData = new Dictionary<string, string>();
            State = FileState.Present;
        }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string MimeType { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public FileState State { get; set; }

        public static FileDbModel FromIFile(IFile file)
        {
            return new FileDbModel()
            {
                Extension = file.Extension,
                Id = file.Id,
                Name = file.Name,
                Path = file.Path,
                Size = file.Size,
                DateCreated = file.DateCreated,
                MetaData = file.MetaData,
                MimeType = file.MimeType,
                DateModified = file.DateModified
            };
        }

        public File ToFile()
        {
            return new File()
            {
                Extension = this.Extension,
                Id = this.Id,
                Name = this.Name,
                Path = this.Path,
                Size = this.Size,
                DateCreated = this.DateCreated,
                MetaData = this.MetaData,
                MimeType = this.MimeType,
                DateModified = this.DateModified
            };
        }

        public string GetHash()
        {
            var strVal = JsonConvert.SerializeObject(this);
            var bytes = Encoding.UTF8.GetBytes(strVal);
            var crypt = SHA512.Create();

            crypt.ComputeHash(bytes);
            
            var sb = new StringBuilder(crypt.HashSize / 4);
            foreach (var b in crypt.Hash)
                sb.AppendFormat("{0:x2}", b);

            return sb.ToString();
        }
    }
}