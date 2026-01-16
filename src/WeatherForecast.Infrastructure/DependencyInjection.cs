using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Builders;
using WeatherForecast.Infrastructure.Caching;
using WeatherForecast.Infrastructure.ExternalApis.OpenMeteo;
using WeatherForecast.Infrastructure.Persistence;
using WeatherForecast.Infrastructure.Persistence.Repositories;

namespace WeatherForecast.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DATABASE
            services.AddDbContext<WeatherForecastContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("WeatherForecastContext")
                    ?? "Data Source=weather. db";

                options.UseSqlite(connectionString);

#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            });

            // REPOSITORIES
            services.AddScoped<IForecastRepository, ForecastRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IWeatherModelRepository, WeatherModelRepository>();

            // EXTERNAL APIs
            services.AddHttpClient<IGeocodingService, OpenMeteoGeocodingService>();
            services.AddHttpClient<IWeatherApiClient, OpenMeteoClient>();
            services.AddScoped<IOpenMeteoParser, OpenMeteoParser>();

            // CACHING
            services.AddScoped<IForecastCacheService, ForecastCacheService>();

            // BUILDERS
            services.AddTransient<IForecastBuilder, ForecastBuilder>();

            return services;
        }

        // DATABASE INITIALIZATION
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WeatherForecastContext>();

            await context.Database.MigrateAsync();
            await SeedWeatherModelsAsync(context);
        }

        private static async Task SeedWeatherModelsAsync(WeatherForecastContext context)
        {
            if (await context.WeatherModels.AnyAsync())
                return;

            var models = new[]
            {
                new Domain.Entities.WeatherModel
                {
                    Name = "ecmwf_ifs025",
                    Provider = "ECMWF",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Domain. Entities.WeatherModel
                {
                    Name = "icon_global",
                    Provider = "DWD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Domain. Entities.WeatherModel
                {
                    Name = "icon_eu",
                    Provider = "DWD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Domain.Entities. WeatherModel
                {
                    Name = "gfs_global",
                    Provider = "NOAA",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Domain. Entities.WeatherModel
                {
                    Name = "arpege_europe",
                    Provider = "Météo-France",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.WeatherModels.AddRange(models);
            await context.SaveChangesAsync();
        }
    }
}