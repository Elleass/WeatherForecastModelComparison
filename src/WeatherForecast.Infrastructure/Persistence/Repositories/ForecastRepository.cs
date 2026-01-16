using Microsoft.EntityFrameworkCore;
using WeatherForecast.Application.Common.Interfaces;  // Interface bêdzie w Application
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository dla operacji na prognozach
    /// Implementuje IForecastRepository (interfejs z Application)
    /// </summary>
    public class ForecastRepository : IForecastRepository
    {
        private readonly WeatherForecastContext _context;

        public ForecastRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        // Pobierz prognozy z ostatnich X godzin (cache)
        public async Task<List<Forecast>> GetRecentForecastAsync(int locationId, DateTime cutoffTime)
        {
            return await _context.Forecasts
                .Include(f => f.WeatherModel)   // JOIN z WeatherModels
                .Include(f => f.Location)       // JOIN z Locations
                .Where(f => f.LocationId == locationId && f.FetchDate >= cutoffTime)
                .OrderBy(f => f.ValidDate)      // Sortuj po dacie prognozy
                .ToListAsync();                 // Wykonaj zapytanie asynchronicznie
        }

        // SprawdŸ czy s¹ œwie¿e prognozy (do cache)
        public async Task<bool> HasRecentForecastAsync(int locationId, DateTime cutoffTime)
        {
            return await _context.Forecasts
                .AnyAsync(f => f.LocationId == locationId && f.FetchDate >= cutoffTime);
        }

        // Zapisz prognozy do bazy
        public async Task SaveForecastAsync(List<Forecast> forecasts)
        {
            _context.Forecasts.AddRange(forecasts);  // Dodaj do kolejki
            await _context.SaveChangesAsync();       // Wykonaj INSERT
        }

        // Usuñ stare prognozy (cleanup)
        public async Task DeleteOldForecastsAsync(DateTime cutoffDate)
        {
            var oldForecasts = await _context.Forecasts
                .Where(f => f.FetchDate < cutoffDate)
                .ToListAsync();

            if (oldForecasts.Any())
            {
                _context.Forecasts.RemoveRange(oldForecasts);
                await _context.SaveChangesAsync();
            }
        }

        // Pobierz najnowsz¹ prognozê dla modelu
        public async Task<Forecast?> GetLatestForecastAsync(int locationId, int weatherModelId)
        {
            return await _context.Forecasts
                .Include(f => f.WeatherModel)
                .Include(f => f.Location)
                .Where(f => f.LocationId == locationId && f.WeatherModelId == weatherModelId)
                .OrderByDescending(f => f.FetchDate)
                .FirstOrDefaultAsync();
        }
    }
}