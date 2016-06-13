using System;
using System.Reflection;

namespace BenchmarkLab.Utility
{
    public static class ApplicationVersionHelper
    {
        public static readonly Lazy<string> ApplicationVersion = new Lazy<string>(()=>GetAppVersion());

        private static string GetAppVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

    }
}
