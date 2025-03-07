using System.Threading.RateLimiting;
using CleanArch.Domain.Exceptions.RateLimitingExceptions;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArch.API.DependencyInjection
{
    public static class RateLimitingConfiguration
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var rateLimitOptions = new RateLimitOptions();
            configuration.GetSection("RateLimiting").Bind(rateLimitOptions);

            services.AddRateLimiter(options =>
            {
                // Add a fixed window limiter for all endpoints
                options.AddFixedWindowLimiter("fixed", options =>
                {
                    options.PermitLimit = rateLimitOptions.PermitLimit;
                    options.Window = TimeSpan.FromSeconds(rateLimitOptions.WindowInSeconds);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateLimitOptions.QueueLimit;
                });

                // Add a sliding window limiter for specific endpoints that need more granular control
                options.AddSlidingWindowLimiter("sliding", options =>
                {
                    options.PermitLimit = rateLimitOptions.SlidingPermitLimit;
                    options.Window = TimeSpan.FromMinutes(rateLimitOptions.SlidingWindowInMinutes);
                    options.SegmentsPerWindow = rateLimitOptions.SegmentsPerWindow;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateLimitOptions.SlidingQueueLimit;
                });

                // Add a token bucket limiter for APIs that need burst capacity
                options.AddTokenBucketLimiter("token", options =>
                {
                    options.TokenLimit = rateLimitOptions.TokenLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateLimitOptions.TokenQueueLimit;
                    options.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.ReplenishmentPeriodInSeconds);
                    options.TokensPerPeriod = rateLimitOptions.TokensPerPeriod;
                    options.AutoReplenishment = true;
                });

                // Configure rate limit exceeded response
                options.OnRejected = async (context, token) =>
                {
                    throw new TooManyRequestsException($"Rate limit exceeded. Try again in {rateLimitOptions.RetryAfterInSeconds} seconds.");
                };
            });

            return services;
        }
    }

    public class RateLimitOptions
    {
        // Fixed Window Options
        public int PermitLimit { get; set; } = 100;
        public int WindowInSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 0;

        // Sliding Window Options
        public int SlidingPermitLimit { get; set; } = 60;
        public int SlidingWindowInMinutes { get; set; } = 1;
        public int SegmentsPerWindow { get; set; } = 6;
        public int SlidingQueueLimit { get; set; } = 0;

        // Token Bucket Options
        public int TokenLimit { get; set; } = 50;
        public int TokenQueueLimit { get; set; } = 0;
        public int ReplenishmentPeriodInSeconds { get; set; } = 10;
        public int TokensPerPeriod { get; set; } = 5;

        // Response Options
        public int RetryAfterInSeconds { get; set; } = 60;
    }
}
