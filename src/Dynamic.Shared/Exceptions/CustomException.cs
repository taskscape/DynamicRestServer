using System;

namespace Dynamic.Shared.Exceptions
{
    public class CustomException : Exception
    {
        protected CustomException(string message) : base(message)
        {
        }
    }
}
