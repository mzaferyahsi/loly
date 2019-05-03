using Loly.Agent.Analysers;
using Xunit;

namespace Loly.Agent.Tests.Analysers
{
    public class FileAnalyserTests
    {
        [Fact]
        public async void AnalyseTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = await analyser.Analyse("./Loly.Agent.Tests.dll");
            Assert.NotNull(fileInfo);
            Assert.Equal("application/x-dosexec", fileInfo.MimeType);
        }
        
        [Fact]
        public async void AnalyseDirectoryTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = await analyser.Analyse("./");
            Assert.NotNull(fileInfo);
        }
        
        [Fact]
        public async void AnalyseFileNotFoundTest()
        {
            var analyser = new FileAnalyser();
            var fileInfo = await analyser.Analyse("./.notfound");
            Assert.Null(fileInfo);
        }
    }
}