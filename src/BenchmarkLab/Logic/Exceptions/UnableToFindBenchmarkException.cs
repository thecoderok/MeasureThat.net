namespace MeasureThat.Net.Exceptions
{
    using System;

    public class UnableToFindBenchmarkException : Exception
    {
        public UnableToFindBenchmarkException(string message): base(message)
        {
        }
    }
}
