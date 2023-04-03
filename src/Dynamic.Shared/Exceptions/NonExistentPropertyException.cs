namespace Dynamic.Shared.Exceptions
{
    public class NonExistentPropertyException : CustomException
    {
        public NonExistentPropertyException(string message) : base(message)
        {
        }
    }
}
