using System;

namespace MeasureThat.Net.Utility
{
    public class LocalAccountsDisabledException : Exception
    {
        public LocalAccountsDisabledException(string message) : base(message) { }
    }
}
