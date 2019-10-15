using System;
using System.Collections.Generic;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace Loly.Analysers
{
    public class ImageMetadataAnalyser : IAnalyser
    {
        private readonly ILogger _logger;

        public ImageMetadataAnalyser(ILogger<ImageMetadataAnalyser> logger)
        {
            _logger = logger;
        }
        public Dictionary<string, string> Analyse(string path)
        {
            try
            {
                var dict = new Dictionary<string, string>();
                var directories = ImageMetadataReader.ReadMetadata(path);

                foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                {
                    if(!dict.ContainsKey($"{directory.Name.Replace(" ", "_")}__{tag.Name.Replace(" ", "_")}"))
                        dict.Add($"{directory.Name.Replace(" ", "_")}__{tag.Name.Replace(" ", "_")}", tag.Description);
                }

                return dict;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when analysing image at {path}", path);
                throw;
            }

        } 

    }
}