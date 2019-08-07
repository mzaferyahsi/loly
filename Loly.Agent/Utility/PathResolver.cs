using System;

namespace Loly.Agent.Utility
{
    public class PathResolver
    {
        public static string Resolve(string path)
        {
            if (path.StartsWith("~/"))
            {
                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                path = path.Replace("~", homePath);
            }

            return path;
        }
    }
}