using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            // Add services to the container.
            builder.Services.AddDbContext<WeatherForecastContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherForecastContext") ?? throw new InvalidOperationException("Connection string 'WeatherForecastContext' not found.")));
            builder.Services.AddHttpClient<IGeocodingService, GeoService>();
            builder.Services.AddScoped<IFetchForecastService, FetchForecast>();
            builder.Services.AddScoped<IForecastRepository, ForecastRepository>();
            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<IWeatherModelRepository, WeatherModelRepository>();
            builder.Services.AddHttpClient();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient",
                    builder => builder
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            var app = builder.Build();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WeatherForecastContext>();
                if (!db.WeatherModel.Any())
                {
                    db.WeatherModel.AddRange(
new WeatherModel { Name = "ecmwf_ifs025", Provider = "ECMWF", IsActive = true, CreatedAt = DateTime.UtcNow },

new WeatherModel { Name = "icon_global", Provider = "DWD", IsActive = true, CreatedAt = DateTime.UtcNow },
new WeatherModel { Name = "icon_eu", Provider = "DWD", IsActive = true, CreatedAt = DateTime.UtcNow },
new WeatherModel { Name = "icon_d2", Provider = "DWD", IsActive = true, CreatedAt = DateTime.UtcNow },

new WeatherModel { Name = "gfs_global", Provider = "NOAA", IsActive = true, CreatedAt = DateTime.UtcNow },

new WeatherModel { Name = "arpege_europe", Provider = "MeteoFrance", IsActive = true, CreatedAt = DateTime.UtcNow }

                    );
                    db.SaveChanges();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseCors("AllowClient");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}