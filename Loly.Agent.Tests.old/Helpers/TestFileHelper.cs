using System.IO;
using Loly.Analysers.Utility;

namespace Loly.Agent.Tests.Helpers
{
    public class TestFileHelper
    {
        public static void Prepare()
        {
            var homePath = PathResolver.Resolve("~/");
            var lolyDirectory = Path.Combine(homePath, "loly");
            if (!Directory.Exists(lolyDirectory)) Directory.CreateDirectory(lolyDirectory);

            var file1 = Path.Combine(lolyDirectory, "file1.txt");
            if (!File.Exists(file1)) File.Create(file1, 1024);

            var file2 = Path.Combine(lolyDirectory, "file2.txt");
            if (!File.Exists(file2)) File.Create(file2, 1024);
        }

        public static void Cleanup()
        {
            var homePath = PathResolver.Resolve("~/");
            var lolyDirectory = Path.Combine(homePath, "loly");
            var file1 = Path.Combine(lolyDirectory, "file1.txt");
            if (File.Exists(file1)) File.Delete(file1);

            var file2 = Path.Combine(lolyDirectory, "file2.txt");
            if (File.Exists(file2)) File.Delete(file2);

            if (Directory.Exists(lolyDirectory)) Directory.Delete(lolyDirectory);
        }
    }
}