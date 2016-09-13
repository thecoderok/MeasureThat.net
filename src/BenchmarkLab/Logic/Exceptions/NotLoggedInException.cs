namespace MeasureThat.Net.Exceptions
{
    using System;

    public class NotLoggedInException : Exception
    {
        public NotLoggedInException(string msg) : base(msg)
        {
        }
    }
}
