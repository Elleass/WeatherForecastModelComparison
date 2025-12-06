using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecastv2.Data;
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
                var context = scope.ServiceProvider.GetRequiredService<WeatherForecastContext>();
                context.Database.EnsureCreated();
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