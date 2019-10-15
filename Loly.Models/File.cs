using System;
using System.Collections.Generic;

namespace Loly.Models
{
    public class File : IFile
    {
        public File()
        {
            MetaData = new Dictionary<string, string>();
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

        public virtual string Id { get; set; }
        public FileState State { get; set; }
    }
}