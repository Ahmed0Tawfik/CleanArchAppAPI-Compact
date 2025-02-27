using CleanArch.API.DependencyInjection;
using CleanArch.API.EndPoints;
using CleanArch.API.Middleware;
using CleanArch.Infrastructure.DependencyInjection;
using Microsoft.Identity.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ADDING DEPENDENCIES
builder.Services.AddAutoRegisterHandlers();
builder.Services.AddFluentValidation();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthenticationServices(builder.Configuration);

// ADDING CORS CONFIGURATION
builder.Services.ConfigureCors();





var app = builder.Build();

// CONFIGURING CORS
app.UseCors("AllowAll");

// OPEN API
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// MIDDLEWARE
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// MAPPING ENDPOINTS
app.MapProductEndPoints();
app.MapAuthEndPoints();


app.Run();