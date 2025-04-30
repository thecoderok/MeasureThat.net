namespace MeasureThat.Net.Logic.Validation
{
    using System;
    using Exceptions;

    public static class Preconditions
    {
        public static void ToBeNonNegative<T>(T val) where T : IComparable<int>
        {
            if (val.CompareTo(0) < 0)
            {
                throw new ValidationException(nameof(val) + " is expected to be non-negative.");
            }
        }

        public static void ToBeNonNegative(long val)
        {
            if (val.CompareTo(0L) < 0)
            {
                throw new ValidationException(nameof(val) + " is expected to be non-negative.");
            }
        }

        public static void ToBePositive<T>(T val) where T : IComparable<int>
        {
            if (val.CompareTo(0) <= 0)
            {
                throw new ValidationException(nameof(val) + " is expected to be greater than zero.");
            }
        }

        public static void NonEmptyString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ValidationException("Given string is empty.");
            }
        }

        public static void ToNotBeNull<T>(T obj)
        {
            if (obj == null)
            {
                throw new ValidationException("Object is null.");
            }
        }

        // Returns valid page number
        public static int ToValidPage(int page)
        {
            if (page < 0)
            {
                return 0;
            }
            return page;
        }
    }
}
