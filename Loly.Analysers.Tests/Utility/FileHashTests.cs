using Loly.Agent.Utility;
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
            var hash = await FileHash.GetSha1Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSha256Test()
        {
            var hash = await FileHash.GetSha256Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSha512Test()
        {
            var hash = await FileHash.GetSha512Hash(GetType().Assembly.Location);
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
    }
}