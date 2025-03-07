namespace CleanArch.Domain.Exceptions.RateLimitingExceptions
{
    public class TooManyRequestsException : AbstractRateLimitingException
    {
        public TooManyRequestsException(string message) : base(message) { }
    }
}
