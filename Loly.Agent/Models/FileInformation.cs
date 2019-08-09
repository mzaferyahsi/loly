using System;
using System.Collections.Generic;

namespace Loly.Agent.Models
{
    public class FileInformation
    {
        public FileInformation()
        {
            MetaData = new Dictionary<string, string>();
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime CratedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MimeType { get; set; }
        public Dictionary<string, string> MetaData { get; set; }

        public string Id { get; set; }
    }
}