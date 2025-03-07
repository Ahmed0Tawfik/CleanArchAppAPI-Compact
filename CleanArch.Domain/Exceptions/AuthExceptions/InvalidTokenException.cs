namespace CleanArch.Domain.Exceptions.AuthExceptions
{
    public class InvalidTokenException : AbstractAuthException
    {
        public InvalidTokenException(string message) : base(message)
        {
        }
    }
}
