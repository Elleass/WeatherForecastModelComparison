using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Models;

namespace WeatherForecastv2.Data
{
    public class WeatherForecastContext : DbContext
    {
        public WeatherForecastContext(DbContextOptions<WeatherForecastContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherForecastv2.Models.Forecast> Forecast { get; set; } = default!;
        public DbSet<WeatherForecastv2.Models.Location> Location { get; set; } = default!;
        public DbSet<WeatherForecastv2.Models.WeatherModel> WeatherModel { get; set; } = default!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Forecast>()
                .HasOne(f => f.Location)
                .WithMany()
                .HasForeignKey(f => f.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Forecast>()
                .HasOne(f => f.WeatherModel)
                .WithMany(w => w.Forecasts)  // FIXED: Reference the navigation property
                .HasForeignKey(f => f.WeatherModelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            modelBuilder.Entity<Location>()
                .Property(l => l.Latitude)
                .HasPrecision(10, 3);

            modelBuilder.Entity<Location>()
                .Property(l => l.Longitude)
                .HasPrecision(10, 3);

            // Configure forecast decimal properties
            modelBuilder.Entity<Forecast>()
                .Property(f => f.Temperature2m)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.ApparentTemperature)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.Precipitation)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.WindSpeed10m)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.Humidity2m)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.PressureSurface)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Forecast>()
                .Property(f => f.Visibility)
                .HasPrecision(8, 2);
        }
    }
}