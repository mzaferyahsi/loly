using System.Collections.Generic;

namespace Loly.App.Models
{
    public interface IDuplicateFile
    {
        string Id { get; set; }
        List<FileInfoLink> Files { get; set; }
        string Hash { get; set; }
    }
}