using System.Collections.Generic;

namespace Loly.Agent.Models
{
    public class FileMetaDataMessage
    {
        public FileMetaDataMessage()
        {
            MetaData = new Dictionary<string, string>();
        }

        public string Path { get; set; }
        public string FileId { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
    }
}