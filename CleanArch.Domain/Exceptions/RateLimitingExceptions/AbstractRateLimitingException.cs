namespace CleanArch.Domain.Exceptions.RateLimitingExceptions
{
    public abstract class AbstractRateLimitingException : Exception
    {
        public AbstractRateLimitingException(string message) : base(message) { }
    }
}
