using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Loly.Streaming.Models
{
    public class FileMetaData
    {
        public FileMetaData()
        {
            MetaData = new Dictionary<string, string>();
        }
        
        public string Path { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public MetadataAction Action { get; set; }
    }

    public enum MetadataAction
    {
        Create,
        Delete,
        Update
    }
}