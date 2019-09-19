//using Xunit;
//
//namespace Loly.Analysers.Tests
//{
//    public class FileAnalyserTests
//    {
//        [Fact]
//        public void AnalyseDirectoryTest()
//        {
//            var analyser = new FileAnalyser();
//            var fileInfo = analyser.Analyse("./");
//            Assert.NotNull(fileInfo);
//        }
//
//        [Fact]
//        public void AnalyseFileNotFoundTest()
//        {
//            var analyser = new FileAnalyser();
//            var fileInfo = analyser.Analyse("./.notfound");
//            Assert.Null(fileInfo);
//        }
//
//        [Fact]
//        public void AnalyseTest()
//        {
//            var analyser = new FileAnalyser();
//            var fileInfo = analyser.Analyse(GetType().Assembly.Location);
//            Assert.NotNull(fileInfo);
//            Assert.Equal("application/x-dosexec", fileInfo.MimeType);
//        }
//    }
//}