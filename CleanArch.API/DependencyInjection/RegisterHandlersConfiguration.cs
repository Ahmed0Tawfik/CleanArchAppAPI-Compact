using CleanArch.Application.RequestHandlingService;
using CleanArch.Application.Common.Interfaces;

namespace CleanArch.API.DependencyInjection
{
    public static class RegisterHandlersConfiguration
    {
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
    }
}
