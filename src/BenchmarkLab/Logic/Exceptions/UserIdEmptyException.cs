namespace MeasureThat.Net.Exceptions
{
    using System;

    public class UserIdEmptyException : Exception
    {
        public UserIdEmptyException(string message) : base(message)
        {
        }
    }
}
