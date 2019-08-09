using Loly.Agent.Analysers;
using Xunit;

namespace Loly.Agent.Tests.Analysers
{
    public class FileAnalyserTests
    {
        [Fact]
        public void AnalyseDirectoryTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = analyser.Analyse("./");
            Assert.NotNull(fileInfo);
        }

        [Fact]
        public void AnalyseFileNotFoundTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = analyser.Analyse("./.notfound");
            Assert.Null(fileInfo);
        }

        [Fact]
        public void AnalyseTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = analyser.Analyse("./Loly.Agent.Tests.dll");
            Assert.NotNull(fileInfo);
            Assert.Equal("application/x-dosexec", fileInfo.MimeType);
        }
    }
}