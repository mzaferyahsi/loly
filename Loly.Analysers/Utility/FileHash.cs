using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Loly.Analysers.Utility
{
    public enum HashMethods
    {
        SHA1,
        Md5,
        SHA256,
        SHA512
    }

    public static class FileHash
    {
        public static async Task<string> GetSHA256Hash(string path)
        {
            return await GetHash<SHA256>(path);
        }

        public static async Task<string> GetSHA512Hash(string path)
        {
            return await GetHash<SHA512>(path);
        }
        public static async Task<string> GetSHA1Hash(string path)
        {
            return await GetHash<SHA1>(path);
        }

        public static async Task<string> GetMD5Hash(string path)
        {
            return await GetHash<MD5>(path);
        }

        public static async Task<string> GetHash<T>(string path) where T : HashAlgorithm
        {
            var sb = new StringBuilder();

            var create = typeof(T).GetMethod("Create", new Type[] { });
            using (var crypt = (T) create.Invoke(null, null))
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var buffer = new byte[8192];
                    int read;

                    // compute the hash on 8KiB blocks
                    while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length)) == buffer.Length)
                        crypt.TransformBlock(buffer, 0, read, buffer, 0);
                    crypt.TransformFinalBlock(buffer, 0, read);

                    // build the hash string
                    sb = new StringBuilder(crypt.HashSize / 4);
                    foreach (var b in crypt.Hash)
                        sb.AppendFormat("{0:x2}", b);
                }
            }

            return sb.ToString();
        }
    }
}