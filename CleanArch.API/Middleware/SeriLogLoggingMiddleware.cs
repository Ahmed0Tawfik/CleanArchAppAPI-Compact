using CleanArch.Infrastructure.DependencyInjection;
using System.Diagnostics;
using System.Text;

namespace CleanArch.API.Middleware
{

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly LoggingOptions _options;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            LoggingOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Don't log if path matches exclusion patterns
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            // Start timing the request
            var sw = Stopwatch.StartNew();

            // Copy the request body for logging
            var requestBody = string.Empty;
            if (_options.LogRequestBody)
            {
                requestBody = await ReadRequestBody(context);
            }

            // Enable response body capture
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Create a correlation ID for tracking the request through the system
            var correlationId = context.TraceIdentifier;
            context.Response.Headers.Add("X-Correlation-ID", correlationId);

            try
            {
                // Log the request
                LogRequest(context, requestBody, correlationId);

                // Process the request
                await _next(context);

                // Reset the stream position to read the response
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                // Copy the response to the original stream
                await responseBodyStream.CopyToAsync(originalBodyStream);

                // Log the response
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await ReadResponseBody(responseBodyStream);
                LogResponse(context, responseBody, sw.ElapsedMilliseconds, correlationId);
            }
            catch (Exception ex)
            {
                // Log exceptions
                LogException(context, ex, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
            finally
            {
                // Restore the original response body stream
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldSkipLogging(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip health checks, metrics, static files, etc.
            if (path == null) return false;

            foreach (var exclusion in _options.ExclusionPatterns)
            {
                if (path.Contains(exclusion.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<string> ReadRequestBody(HttpContext context)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            // Reset the position to allow the request body to be read again
            context.Request.Body.Position = 0;

            return body;
        }

        private async Task<string> ReadResponseBody(Stream bodyStream)
        {
            if (!_options.LogResponseBody)
                return string.Empty;

            using var reader = new StreamReader(
                bodyStream,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            return await reader.ReadToEndAsync();
        }

        private void LogRequest(HttpContext context, string body, string correlationId)
        {
            var requestInfo = new
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                TraceId = Activity.Current?.TraceId.ToString(),
                SpanId = Activity.Current?.SpanId.ToString(),
                Protocol = context.Request.Protocol,
                Method = context.Request.Method,
                Scheme = context.Request.Scheme,
                Host = context.Request.Host.ToString(),
                Path = context.Request.Path.ToString(),
                QueryString = context.Request.QueryString.ToString(),
                Headers = FilterSensitiveHeaders(context.Request.Headers),
                Body = _options.LogRequestBody && !string.IsNullOrEmpty(body) ? SanitizeBody(body) : null,
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                User = context.User?.Identity?.Name ?? "anonymous"
            };

            _logger.LogInformation("HTTP Request {Method} {Path} received from {IPAddress} - {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                correlationId);

            if (_options.LogDetailedRequests)
            {
                _logger.LogDebug("Request details: {@RequestInfo}", requestInfo);
            }
        }

        private void LogResponse(HttpContext context, string body, long elapsedMs, string correlationId)
        {
            var responseInfo = new
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                TraceId = Activity.Current?.TraceId.ToString(),
                SpanId = Activity.Current?.SpanId.ToString(),
                StatusCode = context.Response.StatusCode,
                Headers = FilterSensitiveHeaders(context.Response.Headers),
                Body = _options.LogResponseBody && !string.IsNullOrEmpty(body) ? SanitizeBody(body) : null,
                ElapsedMilliseconds = elapsedMs
            };

            _logger.LogInformation("HTTP Response {StatusCode} for {Method} {Path} completed in {ElapsedMilliseconds}ms - {CorrelationId}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                correlationId);

            if (_options.LogDetailedResponses)
            {
                _logger.LogDebug("Response details: {@ResponseInfo}", responseInfo);
            }
        }

        private void LogException(HttpContext context, Exception ex, long elapsedMs, string correlationId)
        {
            _logger.LogError(ex, "HTTP Request {Method} {Path} failed after {ElapsedMilliseconds}ms with error: {ErrorMessage} - {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                ex.Message,
                correlationId);
        }

        private Dictionary<string, string> FilterSensitiveHeaders(IHeaderDictionary headers)
        {
            var filteredHeaders = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                var key = header.Key.ToLower();

                // Skip sensitive headers
                if (_options.SensitiveHeaders.Contains(key))
                {
                    filteredHeaders.Add(header.Key, "[REDACTED]");
                }
                else
                {
                    filteredHeaders.Add(header.Key, header.Value.ToString());
                }
            }

            return filteredHeaders;
        }

        private string SanitizeBody(string body)
        {
            // Prevent logging of overly large payloads
            if (body.Length > _options.MaxBodyLogSize)
            {
                return $"[TRUNCATED to {_options.MaxBodyLogSize} chars]" + body.Substring(0, _options.MaxBodyLogSize);
            }

            // TODO: Add logic to sanitize sensitive data (e.g., PII, credit cards, tokens)
            // This would be application-specific, so implement based on your needs

            return body;
        }
    }

    public static class SeriLogLoggingMiddleware
    { 
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
