using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Loly.Agent.Utility
{
    public enum HashMethods
    {
        Sha1,
        Md5
    }

    public static class FileHash
    {
        public static async Task<string> GetSha1Hash(string path)
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