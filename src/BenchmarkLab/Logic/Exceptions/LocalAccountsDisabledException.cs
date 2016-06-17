using System;

namespace BenchmarkLab.Utility
{
    public class LocalAccountsDisabledException : Exception
    {
        public LocalAccountsDisabledException(string message) : base(message) { }
    }
}
