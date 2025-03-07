using CleanArch.API.DependencyInjection;
using CleanArch.API.EndPoints;
using CleanArch.API.Middleware;
using CleanArch.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ADDING DEPENDENCIES

// ADDING AUTO REGISTER HANDLERS
builder.Services.AddAutoRegisterHandlers();

// ADDING FLUENT VALIDATION
builder.Services.AddFluentValidation();


// ADDING INFRASTRUCTURE SERVICES
builder.Services.AddInfrastructureServices(builder.Configuration);


// ADDING IDENTITY SERVICES
builder.Services.AddIdentityServices();

// ADDING AUTHENTICATION SERVICES
builder.Services.AddAuthenticationServices(builder.Configuration);

// ADDING CORS CONFIGURATION
builder.Services.ConfigureCors();

// ADDING RATE LIMITING
builder.Services.AddRateLimiting(builder.Configuration);

var app = builder.Build();






// CONFIGURING CORS
app.UseCors("AllowAll");

// OPEN API
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// GLOBAL EXCEPTION HANDLING MIDDLEWARE
app.UseExceptionHandling();


app.UseHttpsRedirection();

// ADDING RATE LIMITING MIDDLEWARE
app.UseRateLimiter();

// AUTHENTICATION AND AUTHORIZATION
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// MAPPING ENDPOINTS
app.MapProductEndPoints();
app.MapAuthEndPoints();


app.Run();