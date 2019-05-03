using System;
using System.IO;
using System.Threading.Tasks;
using HeyRed.Mime;
using log4net;
using Loly.Agent.Models;
using Loly.Agent.Utility;

namespace Loly.Agent.Analysers
{
    public class FileAnalyser : IAnalyser
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(FileAnalyser));

        public async Task<FileInformation> Analyse(string path)
        {
            try
            {
                path = path.Trim('\"');
                var fileAttr = File.GetAttributes(path);

                FileInformation information = (fileAttr & FileAttributes.Directory) != 0
                    ? await GetDirectoryInformation(Path.GetFullPath(path))
                    : await GetFileInformation(Path.GetFullPath(path));

                return information;
            }
            catch (FileNotFoundException e)
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

        private async Task<FileInformation> GetDirectoryInformation(string path)
        {
            try
            {
                var fsFileInfo = new DirectoryInfo(path);

                if (!fsFileInfo.Exists)
                    throw new FileNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);

                var fileInfo = new FileInformation()
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
            catch (FileNotFoundException e)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
        }

        private async Task<FileInformation> GetFileInformation(string path)
        {
            try
            {
                var fsFileInfo = new FileInfo(path);

                if (!fsFileInfo.Exists)
                    throw new FileNotFoundException();

                var mimeType = MimeGuesser.GuessMimeType(path);

                string hash = await FileHash.GetMD5Hash(path);

                var fileInfo = new FileInformation()
                {
                    Name = fsFileInfo.Name,
                    Extension = fsFileInfo.Extension,
                    CratedDate = fsFileInfo.CreationTimeUtc,
                    ModifiedDate = fsFileInfo.LastWriteTimeUtc,
                    Size = fsFileInfo.Length,
                    Path = path,
                    MimeType = mimeType,
                    Hash = hash
                };

                return fileInfo;
            }
            catch (FileNotFoundException e)
            {
                _log.Warn($"Unable to find {path}");
                return null;
            }
        }
    }
}