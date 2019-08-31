using Loly.Analysers.Utility;
using Xunit;

namespace Loly.Analysers.Tests.Utility
{
    public class FileHashTests
    {
        [Fact]
        public async void HashWithMd5Test()
        {
            var hash = await FileHash.GetMD5Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }

        [Fact]
        public async void HashWithSha1Test()
        {
            var hash = await FileHash.GetSHA1Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSha256Test()
        {
            var hash = await FileHash.GetSHA256Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSha512Test()
        {
            var hash = await FileHash.GetSHA512Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
    }
}