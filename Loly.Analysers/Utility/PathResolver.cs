using System;

namespace Loly.Analysers.Utility
{
    public static class PathResolver
    {
        public static string Resolve(string path)
        {
            if (!path.StartsWith("~/")) return path;
            
            var homePath = Environment.OSVersion.Platform == PlatformID.Unix ||
                           Environment.OSVersion.Platform == PlatformID.MacOSX
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            path = path.Replace("~", homePath);

            return path;
        }
    }
}