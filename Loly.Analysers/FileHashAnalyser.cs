using System;
using System.IO;
using System.Threading.Tasks;
using Loly.Analysers.Utility;
using Microsoft.Extensions.Logging;

namespace Loly.Analysers
{
    public class FileHashAnalyser : IAnalyser
    {
        private readonly ILogger _log;

        public FileHashAnalyser( ILogger<FileHashAnalyser> logger)
        {
            this._log = logger;
        }

        public async Task<string> Analyse(HashMethods method, string path)
        {
            switch (method)
            {
                case HashMethods.SHA1:
                    return await GenerateSHA1Hash(path);
                case HashMethods.Md5:
                    return await GenerateMd5Hash(path);
                case HashMethods.SHA256:
                    return await GenerateSHA256Hash(path);
                case HashMethods.SHA512:
                    return await GenerateSHA512Hash(path);
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, "Hash method not supported.");
            }
        }

        public async Task<string> GenerateSHA1Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSHA1Hash(path);
                return hash;
            }
            catch (FileNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when generating file hash");
                return null;
            }
        }
        
        public async Task<string> GenerateSHA256Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSHA256Hash(path);
                return hash;
            }
            catch (FileNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when generating file hash");
                return null;
            }
        }
        
        public async Task<string> GenerateSHA512Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSHA512Hash(path);
                return hash;
            }
            catch (FileNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when generating file hash");
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

                var hash = await FileHash.GetMD5Hash(path);
                return hash;
            }
            catch (FileNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                _log.LogWarning($"Unable to find {path}");
                return null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when generating file hash");
                return null;
            }
        }
    }
}