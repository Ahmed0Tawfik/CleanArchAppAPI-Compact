using System.Net;

namespace CleanArch.Application.Common.APIResponse
{
    public class ApiResponse<T>
    {
        public bool IsError { get; set; } // Indicates success or failure
        public T? Payload { get; set; } // The actual response data

        public string Message { get; set; } // Message (if any) 
        public object? Errors { get; set; } // List of error messages (if any)
        public HttpStatusCode StatusCode { get; set; } // HTTP status code

        //  Success response constructor
        public static ApiResponse<T> Success(T payload, string message = "Operation Success", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse<T>
            {
                IsError = false,
                Payload = payload,
                Message = message,
                Errors = null,
                StatusCode = statusCode,
            };
        }

        //  Error response constructor
        public static ApiResponse<T> Error(object errors, string message = "Operation Failure", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>
            {
                IsError = true,
                Payload = default,
                Message = message,
                Errors = errors,
                StatusCode = statusCode,
            };
        }


    }
}
