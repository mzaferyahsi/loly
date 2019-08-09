using System;
using System.IO;
using HeyRed.Mime;
using log4net;
using Loly.Agent.Models;

namespace Loly.Agent.Analysers
{
    public class FileAnalyser : IAnalyser
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(FileAnalyser));

        public FileInformation Analyse(string path)
        {
            try
            {
                path = path.Trim('\"');
                var fileAttr = File.GetAttributes(path);

                var information = (fileAttr & FileAttributes.Directory) != 0
                    ? GetDirectoryInformation(Path.GetFullPath(path))
                    : GetFileInformation(Path.GetFullPath(path));

                return information;
            }
            catch (FileNotFoundException)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _log.Error(e);
                return null;
            }
        }

        private FileInformation GetDirectoryInformation(string path)
        {
            try
            {
                var fsFileInfo = new DirectoryInfo(path);

                if (!fsFileInfo.Exists)
                    throw new DirectoryNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);

                var fileInfo = new FileInformation
                {
                    Name = fsFileInfo.Name,
                    Extension = fsFileInfo.Extension,
                    CratedDate = fsFileInfo.CreationTimeUtc,
                    ModifiedDate = fsFileInfo.LastWriteTimeUtc,
                    Size = -1,
                    Path = path,
                    MimeType = mimeType
                };

                return fileInfo;
            }
            catch (FileNotFoundException)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
        }

        private FileInformation GetFileInformation(string path)
        {
            try
            {
                var fsFileInfo = new FileInfo(path);

                if (!fsFileInfo.Exists)
                    throw new FileNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);


                var fileInfo = new FileInformation
                {
                    Name = fsFileInfo.Name,
                    Extension = fsFileInfo.Extension,
                    CratedDate = fsFileInfo.CreationTimeUtc,
                    ModifiedDate = fsFileInfo.LastWriteTimeUtc,
                    Size = fsFileInfo.Length,
                    Path = path,
                    MimeType = mimeType
                };

                return fileInfo;
            }
            catch (FileNotFoundException)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
        }
    }
}