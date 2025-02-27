using CleanArch.Infrastructure.Context;
using CleanArch.Infrastructure.Repositories;
using CleanArch.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CleanArch.Application.Interfaces.Authentication;
using CleanArch.Infrastructure.Services.Authentication;
using CleanArch.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CleanArch.Domain;
using System.Globalization;

namespace CleanArch.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
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


            services.AddScoped<IAuthService,AuthService>();
            services.AddScoped<ITokenService,TokenService>();
        }

        public static void AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"])),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
            }

        }
}
