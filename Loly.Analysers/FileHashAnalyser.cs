using System;
using System.IO;
using System.Threading.Tasks;
using Loly.Analysers.Utility;
using Microsoft.Extensions.Logging;

namespace Loly.Analysers
{
    public class FileHashAnalyser : IAnalyser
    {
        private readonly ILogger _logger;

        public FileHashAnalyser( ILogger<FileHashAnalyser> logger)
        {
            this._logger = logger;
        }

        public async Task<string> Analyse(HashMethods method, string path)
        {
            switch (method)
            {
                case HashMethods.Sha1:
                    return await GenerateSha1Hash(path);
                case HashMethods.Md5:
                    return await GenerateMd5Hash(path);
                case HashMethods.Sha256:
                    return await GenerateSha256Hash(path);
                case HashMethods.Sha512:
                    return await GenerateSha512Hash(path);
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
                _logger.LogError(e, "Error when generating file hash");
                return null;
            }
        }
        
        public async Task<string> GenerateSha256Hash(string path)
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
                _logger.LogError(e, "Error when generating file hash");
                return null;
            }
        }
        
        public async Task<string> GenerateSha512Hash(string path)
        {
            try
            {
                path = path.Trim('\"');
                var resolvedPath = PathResolver.Resolve(path);
                var fileAttr = File.GetAttributes(resolvedPath);
                if ((fileAttr & FileAttributes.Directory) != 0) return string.Empty;

                var hash = await FileHash.GetSha512Hash(path);
                return hash;
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
                _logger.LogError(e, "Error when generating file hash");
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

                var hash = await FileHash.GetMd5Hash(path);
                return hash;
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
                _logger.LogError(e, "Error when generating file hash");
                return null;
            }
        }
    }
}