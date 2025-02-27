using CleanArch.Application;
using CleanArch.Application.Auth;
using CleanArch.Application.RequestHandlingService;
using FluentValidation;

namespace CleanArch.API.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserLoginCommand.Validator>();
            return services;
        }

        public static IServiceCollection AddAutoRegisterHandlers(this IServiceCollection services)
        {
            services.AddScoped<IRequestSender, RequestSender>();

            var handlerInterface = typeof(IRequestHandler<,>);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)))
                {
                    foreach (var interfaceType in type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface))
                    {
                        services.AddTransient(interfaceType, type);
                    }
                }
            }
            return services;
        }

        public static IServiceCollection ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            return services;


        }
    }
}
