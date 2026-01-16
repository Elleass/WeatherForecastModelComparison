using Microsoft.EntityFrameworkCore;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence
{

    public class WeatherForecastContext : DbContext
    {
        // Konstruktor - przyjmuje opcje (connection string, provider)
        public WeatherForecastContext(DbContextOptions<WeatherForecastContext> options)
            : base(options)
        {
        }

        // DbSet = tabela w bazie danych
        // Forecasts tabela "Forecasts" z wierszami typu Forecast
        public DbSet<Forecast> Forecasts { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<WeatherModel> WeatherModels { get; set; } = null!;

        // OnModelCreating - konfiguruje jak tabele wygl¹daj¹
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Zastosuj wszystkie konfiguracje (Configurations/)
            // Zamiast pisaæ tutaj 200 linii, rozdzielamy na osobne pliki
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WeatherForecastContext).Assembly);
        }
    }
}