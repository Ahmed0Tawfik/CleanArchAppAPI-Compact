using System.Net;

namespace CleanArch.Domain.Exceptions.AuthExceptions
{
    public abstract class AbstractAuthException : Exception
    {
        public AbstractAuthException(string message) : base(message)
        {
        }
    }
}
