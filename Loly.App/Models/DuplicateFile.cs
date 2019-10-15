using System.Collections.Generic;
using Loly.App.Db.Models;
using Microsoft.AspNetCore.Mvc;

namespace Loly.App.Models
{
    public class DuplicateFile : IDuplicateFile
    {
        public string Id { get; set; }
        public List<FileInfoLink> Files { get; set; }
        public string Hash { get; set; }

        public static DuplicateFile FromDbModel(IUrlHelper urlHelper, DuplicateFileDbModel model)
        {
            var dto = new DuplicateFile()
            {
                Hash = model.Hash,
                Id = model.Id,
                Files = new List<FileInfoLink>()
            };

            foreach (var fileReference in model.Files)
            {
                dto.Files.Add(new FileInfoLink( urlHelper.Link("GetFile", new { id = fileReference.Id}),"self", "GET"));
            }

            return dto;
        }
    }
}