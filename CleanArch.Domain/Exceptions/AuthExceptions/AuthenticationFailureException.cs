namespace CleanArch.Domain.Exceptions.AuthExceptions
{
    public class AuthenticationFailureException : AbstractAuthException
    {
        public AuthenticationFailureException(string message) : base(message)
        {
        }
    }
}
