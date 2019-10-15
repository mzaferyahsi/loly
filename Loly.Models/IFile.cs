using System;
using System.Collections.Generic;

namespace Loly.Models
{
    public interface IFile
    {
        string Path { get; set; }
        string Name { get; set; }
        string Extension { get; set; }
        long Size { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        string MimeType { get; set; }
        Dictionary<string, string> MetaData { get; set; }
        string Id { get; set; }
        FileState State { get; set; }
    }
}