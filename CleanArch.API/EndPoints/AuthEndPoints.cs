using CleanArch.Application.Features.Auth.Commands;
using CleanArch.Application.RequestHandlingService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.API.EndPoints
{
    public static class AuthEndPoint
    {
        
        public static void MapAuthEndPoints(this IEndpointRouteBuilder app)
        {
            var endpoints = app.MapGroup("/Auth")
                .WithOpenApi();


            endpoints.MapPost("/login", async (
                [FromBody] UserLoginCommand request,
                [FromServices] IRequestSender  handler,
                IValidator<UserLoginCommand> validator,
                CancellationToken ct) =>
            {
                await validator.ValidateAndThrowAsync(request, ct);
                return await handler.Send(request, ct);
            })
            .RequireRateLimiting("sliding")
            .WithSummary("Login User");


            endpoints.MapPost("/register", async (
                 [FromBody] UserRegisterCommand request,
                 [FromServices] IRequestSender handler,
                 IValidator<UserRegisterCommand> validator,
                 CancellationToken ct) =>
            {
                await validator.ValidateAndThrowAsync(request, ct);
                return await handler.Send(request, ct);
            })
            .RequireRateLimiting("sliding")
            .WithSummary("Register User");
        }
    }
}
