namespace BenchmarkLab.Logic.Exceptions
{
    using System;

    public class ValidationException: Exception
    {
        public ValidationException(string message) : base(message)
        {
            
        }
    }
}
