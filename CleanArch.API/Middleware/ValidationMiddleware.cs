using FluentValidation;

namespace CleanArch.API.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            foreach (var metadata in endpoint.Metadata)
            {
                if (metadata is IValidator validator)
                {
                    var model = await context.Request.ReadFromJsonAsync(validator.GetType().GenericTypeArguments[0]);
                    var validationResult = validator.Validate(new ValidationContext<object>(model));

                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }
                }
            }

            await _next(context);
        }
    }
}
