using Microsoft.EntityFrameworkCore;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Repositories  // ← POPRAWKA:  namespace
{
    /// <summary>
    /// Repository dla operacji na lokalizacjach (miastach)
    /// </summary>
    public class LocationRepository : ILocationRepository
    {
        private readonly WeatherForecastContext _context;

        public LocationRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        // Znajdź lokalizację po nazwie miasta
        public async Task<Location?> GetByCityNameAsync(string cityName)
        {
            return await _context.Locations  // ← POPRAWKA:  Locations (множина)
                .FirstOrDefaultAsync(l => l.Name == cityName);
        }

        // ← DODANE: Metoda do dodawania nowej lokalizacji
        // Przydatne gdy GeocodingService znajdzie nowe miasto
        public async Task<Location> AddAsync(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }
    }
}