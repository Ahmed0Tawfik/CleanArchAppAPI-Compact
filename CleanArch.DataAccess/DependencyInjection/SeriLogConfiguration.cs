using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Hosting;

namespace CleanArch.Infrastructure.DependencyInjection
{
    public static class SeriLogConfiguration
    {
        public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var loggingOptions = new LoggingOptions();
            configuration.GetSection("Logging:Middleware").Bind(loggingOptions);
            services.AddSingleton(loggingOptions);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                //.WriteTo.File(
                //    path: "logs/app-.log",
                //    rollingInterval: RollingInterval.Day,
                //    fileSizeLimitBytes: 10 * 1024 * 1024,
                //    retainedFileCountLimit: 30,
                //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            return services;
        }
    }

    public class LoggingOptions
    {
        public bool LogRequestBody { get; set; } = true;
        public bool LogResponseBody { get; set; } = true;
        public bool LogDetailedRequests { get; set; } = true;
        public bool LogDetailedResponses { get; set; } = true;
        public int MaxBodyLogSize { get; set; } = 4096;
        public List<string> ExclusionPatterns { get; set; } = new List<string>
        {
            "/health",
            "/metrics",
            "/swagger",
            "/static",
            "/favicon.ico"
        };
        public List<string> SensitiveHeaders { get; set; } = new List<string>
        {
            "authorization",
            "cookie",
            "x-api-key",
            "x-csrf-token"
        };
    }
}
