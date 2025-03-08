using CleanArch.Application.Interfaces.Authentication;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain;
using CleanArch.Infrastructure.Context;
using CleanArch.Infrastructure.Repositories;
using CleanArch.Infrastructure.Services.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesConfiguration
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add this in your service configuration section
            services.AddOptions<JwtSettings>()
                 .Configure<IConfiguration>((settings, config) =>
                 {
                     config.GetSection("JwtSettings").Bind(settings);
                     settings.TokenLifetime = TimeSpan.ParseExact(
                         config["JwtSettings:TokenLifetime"]!,
                         @"hh\:mm\:ss",
                         CultureInfo.InvariantCulture
                     );
                 });

            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(
                     configuration.GetConnectionString("DefaultConnection"),
                     b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
        }
    }
}
