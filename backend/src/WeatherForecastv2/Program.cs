using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using WeatherForecastv2.Data;
using WeatherForecastv2.Models;
using WeatherForecastv2.Repositories;
using WeatherForecastv2.Services;

namespace WeatherForecastv2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            /* =========================
               Services
               ========================= */

            builder.Services.AddDbContext<WeatherForecastContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("WeatherForecastContext")
                    ?? throw new InvalidOperationException("Connection string not found.")
                )
            );

            builder.Services.AddHttpClient<IGeocodingService, GeoService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IFetchForecastService, FetchForecast>();
            builder.Services.AddScoped<IForecastRepository, ForecastRepository>();
            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<IWeatherModelRepository, WeatherModelRepository>();

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            /* =========================
               CORS
               ========================= */

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", policy =>
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                );
            });

            /* =========================
               RATE LIMITING (CRITICAL)
               ========================= */

            builder.Services.AddRateLimiter(options =>
            {
                // Default limiter for all endpoints
                options.AddFixedWindowLimiter("api", limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.PermitLimit = 60; // 60 req/min/IP
                    limiter.QueueLimit = 0;
                });

                // Stricter limiter for endpoints that hit external APIs
                options.AddFixedWindowLimiter("external", limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.PermitLimit = 15; // PROTECT your free API quota
                    limiter.QueueLimit = 0;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            var app = builder.Build();

            /* =========================
               DATABASE SEEDING
               ========================= */

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WeatherForecastContext>();
                if (!db.WeatherModel.Any())
                {
                    db.WeatherModel.AddRange(
                        new WeatherModel { Name = "ecmwf_ifs025", Provider = "ECMWF", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new WeatherModel { Name = "icon_global", Provider = "DWD", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new WeatherModel { Name = "icon_eu", Provider = "DWD", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new WeatherModel { Name = "gfs_global", Provider = "NOAA", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new WeatherModel { Name = "arpege_europe", Provider = "MeteoFrance", IsActive = true, CreatedAt = DateTime.UtcNow }
                    );
                    db.SaveChanges();
                }
            }

            /* =========================
               HTTP PIPELINE
               ========================= */

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Needed if behind proxy / Docker / Cloudflare
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors("AllowClient");

            // Cheap bot filter (blocks dumb scripts early)
            app.Use(async (context, next) =>
            {
                var ua = context.Request.Headers.UserAgent.ToString();

                if (string.IsNullOrWhiteSpace(ua) || ua.Length < 8)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }

                await next();
            });

            app.UseRateLimiter();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            // Apply default limiter to ALL controllers
            app.MapControllers()
               .RequireRateLimiting("api");

            app.Run();
        }
    }
}
