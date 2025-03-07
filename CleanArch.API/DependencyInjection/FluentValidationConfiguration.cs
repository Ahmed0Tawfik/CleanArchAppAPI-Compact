using CleanArch.Application;
using CleanArch.Application.Features.Auth.Commands;
using CleanArch.Application.RequestHandlingService;
using FluentValidation;

namespace CleanArch.API.DependencyInjection
{
    public static class FluentValidationConfiguration
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserLoginCommand.Validator>();
            return services;
        }

        

        
    }
}
