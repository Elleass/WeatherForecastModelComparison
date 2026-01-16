using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Konfiguracja tabeli Forecasts
    /// - Typy kolumn (precyzja dla decimal)
    /// - Relacje (Foreign Keys)
    /// - Indeksy (dla wydajnoœci)
    /// </summary>
    public class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
    {
        public void Configure(EntityTypeBuilder<Forecast> builder)
        {
            // Nazwa tabeli
            builder.ToTable("Forecasts");

            // Klucz g³ówny
            builder.HasKey(f => f.Id);

            // RELACJE (Foreign Keys)

            // Forecast nale¿y do Location (wiele prognoz dla jednej lokalizacji)
            builder.HasOne(f => f.Location)          // Forecast ma jedn¹ Location
                .WithMany(l => l.Forecasts)          // Location ma wiele Forecasts
                .HasForeignKey(f => f.LocationId)    // Klucz obcy
                .OnDelete(DeleteBehavior.Cascade);   // Usuñ prognozy gdy lokalizacja zostanie usuniêta

            // Forecast u¿ywa WeatherModel
            builder.HasOne(f => f.WeatherModel)
                .WithMany(w => w.Forecasts)
                .HasForeignKey(f => f.WeatherModelId)
                .OnDelete(DeleteBehavior.Cascade);

            // PRECYZJA dla liczb zmiennoprzecinkowych
            // HasPrecision(5, 2) = 5 cyfr total, 2 po przecinku  np. 123.45

            builder.Property(f => f.Temperature2m)
                .HasPrecision(5, 2);    // -99.99 do 999.99

            builder.Property(f => f.ApparentTemperature)
                .HasPrecision(5, 2);

            builder.Property(f => f.Precipitation)
                .HasPrecision(5, 2);    // Opady w mm

            builder.Property(f => f.WindSpeed10m)
                .HasPrecision(5, 2);    // Wiatr w m/s

            builder.Property(f => f.Humidity2m)
                .HasPrecision(5, 2);    // Wilgotnoœæ 

            builder.Property(f => f.PressureSurface)
                .HasPrecision(8, 2);    // Ciœnienie w hPa (1013. 25)

            builder.Property(f => f.Visibility)
                .HasPrecision(8, 2);    // Widocznoœæ w metrach

            // INDEKSY - przyspieszaj¹ zapytania

            // Czêsto szukamy prognoz po LocationId + FetchDate
            builder.HasIndex(f => new { f.LocationId, f.FetchDate });

            // Czêsto filtrujemy po ValidDate (data prognozy)
            builder.HasIndex(f => f.ValidDate);
        }
    }
}