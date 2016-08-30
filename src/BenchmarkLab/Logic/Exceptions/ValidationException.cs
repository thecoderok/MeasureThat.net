namespace MeasureThat.Net.Logic.Exceptions
{
    using System;

    public class ValidationException: Exception
    {
        public ValidationException(string message) : base(message)
        {
            
        }
    }
}
