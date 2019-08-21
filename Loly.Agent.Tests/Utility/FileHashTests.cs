using Loly.Agent.Utility;
using Xunit;

namespace Loly.Agent.Tests.Utility
{
    public class FileHashTests
    {
        [Fact]
        public async void HashWithMD5Test()
        {
            var hash = await FileHash.GetMD5Hash("./Loly.Agent.Tests.dll");
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }

        [Fact]
        public async void HashWithSHA1Test()
        {
            var hash = await FileHash.GetSha1Hash("./Loly.Agent.Tests.dll");
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSHA256Test()
        {
            var hash = await FileHash.GetSha256Hash("./Loly.Agent.Tests.dll");
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
        [Fact]
        public async void HashWithSHA512Test()
        {
            var hash = await FileHash.GetSha512Hash("./Loly.Agent.Tests.dll");
            Assert.IsType<string>(hash);
            Assert.NotEqual(string.Empty, hash);
        }
    }
}