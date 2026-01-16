using WeatherForecast.Application;
using WeatherForecast.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// SERVICES REGISTRATION

// Clean Architecture layers (u¿ywamy extension methods)
builder.Services.AddApplication();      // Facade + Handlers
builder.Services.AddInfrastructure(builder.Configuration);  // Repositories + External APIs

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - dla frontendu (localhost: 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// RATE LIMITING - ochrona API
builder.Services.AddRateLimiter(options =>
{
    // Ogólny limit - 60 req/min
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 60;
        limiter.QueueLimit = 0;
    });

    // Bardziej restrykcyjny dla endpointów z external API
    options.AddFixedWindowLimiter("forecast", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 10;  // Tylko 10 req/min (ochrona limitu Open-Meteo)
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// DATABASE INITIALIZATION (migracje + seed)
using (var scope = app.Services.CreateScope())
{
    await WeatherForecast.Infrastructure.DependencyInjection
        .InitializeDatabaseAsync(scope.ServiceProvider);
}

// MIDDLEWARE PIPELINE


// Swagger (tylko w development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();