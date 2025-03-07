using CleanArch.Application.APIResponse;
using CleanArch.Domain.Exceptions.AuthExceptions;
using CleanArch.Domain.Exceptions.RateLimitingExceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace CleanArch.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var response = CreateErrorResponse(exception, statusCode);

            await WriteResponseAsync(context, statusCode, response);
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                AbstractRateLimitingException => HttpStatusCode.TooManyRequests,
                AbstractAuthException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static object CreateErrorResponse(Exception exception, HttpStatusCode statusCode)
        {
            return exception switch
            {
                ValidationException validationException =>
                    ApiResponse<List<object>>.Error(
                        validationException.Errors.Select(err => new { err.PropertyName, err.ErrorMessage }).ToList(),
                        "Validation failed",
                        statusCode),

                _ => ApiResponse<string>.Error(
                    new List<object> { exception.Message },
                    GetDefaultErrorMessage(statusCode),
                    statusCode)
            };
        }

        private static string GetDefaultErrorMessage(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Invalid request",
                HttpStatusCode.Unauthorized => "Authentication required",
                HttpStatusCode.TooManyRequests => "Rate limit exceeded",
                _ => "An unexpected error occurred"
            };
        }

        private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, object response)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsJsonAsync(response);
        }
    }

    // Extension method for cleaner registration in program.cs
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}