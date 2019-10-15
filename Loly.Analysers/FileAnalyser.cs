using System;
using System.IO;
using HeyRed.Mime;
using Microsoft.Extensions.Logging;
using File = Loly.Models.File;

namespace Loly.Analysers
{
    public class FileAnalyser : IAnalyser
    {
        private readonly ILogger _logger;

        public FileAnalyser(ILogger<FileAnalyser> logger)
        {
            _logger = logger;
        }

        public File Analyse(string path)
        {
            try
            {
                path = path.Trim('\"');
                var fileAttr = System.IO.File.GetAttributes(path);

                var information = (fileAttr & FileAttributes.Directory) != 0
                    ? GetDirectoryInformation(Path.GetFullPath(path))
                    : GetFileInformation(Path.GetFullPath(path));

                return information;
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _logger.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when analysing file.");
                return null;
            }
        }

        private File GetDirectoryInformation(string path)
        {
            try
            {
                var fsFileInfo = new DirectoryInfo(path);

                if (!fsFileInfo.Exists)
                    throw new DirectoryNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);

                var fileInfo = new File
                {
                    Name = fsFileInfo.Name,
                    Extension = fsFileInfo.Extension,
                    DateCreated = fsFileInfo.CreationTimeUtc,
                    DateModified = fsFileInfo.LastWriteTimeUtc,
                    Size = -1,
                    Path = path,
                    MimeType = mimeType
                };

                return fileInfo;
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning($"Unable to find {path}");
                return null;
            }
        }

        private File GetFileInformation(string path)
        {
            try
            {
                var fsFileInfo = new FileInfo(path);

                if (!fsFileInfo.Exists)
                    throw new FileNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);


                var fileInfo = new File
                {
                    Name = fsFileInfo.Name,
                    Extension = fsFileInfo.Extension,
                    DateCreated = fsFileInfo.CreationTimeUtc,
                    DateModified = fsFileInfo.LastWriteTimeUtc,
                    Size = fsFileInfo.Length,
                    Path = path,
                    MimeType = mimeType
                };

                return fileInfo;
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning($"Unable to find {path}");
                return null;
            }
        }
    }
}