using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Loly.Agent.Utility;

namespace Loly.Analysers
{
    public class FileHashAnalyser : IAnalyser
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(FileHashAnalyser));

        public async Task<string> Analyse(HashMethods method, string path)
        {
            switch (method)
            {
                case HashMethods.Sha1:
                    return await GenerateSha1Hash(path);
                case HashMethods.Md5:
                    return await GenerateMd5Hash(path);
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, "Hash method not supported.");
            }
        }

        public async Task<string> GenerateSha1Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSha1Hash(path);
                return hash;
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

        public async Task<string> GenerateMd5Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSha256Hash(path);
                return hash;
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
    }
}