namespace MeasureThat.Net.Exceptions
{
    using System;

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {

        }
    }
}
