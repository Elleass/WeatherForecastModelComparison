using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;
using WeatherForecastv2.Models;

namespace WeatherForecastv2.Repositories
{
    public class ForecastRepository : IForecastRepository
    {
        private readonly WeatherForecastContext _context;  // Added 'private' modifier

        public ForecastRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        public async Task<List<Forecast>> GetRecentForecastAsync(int locationId, DateTime cutoffTime)
        {
            return await _context.Forecast
                .Include(f => f.WeatherModel)  // Include navigation property
                .Include(f => f.Location)      // Include navigation property
                .Where(f => f.LocationId == locationId && f.FetchDate >= cutoffTime)
                .OrderBy(f => f.ValidDate)
                .ToListAsync();
        }

        public async Task<List<Forecast>> GetForecastByLocationAndModels(int locationId, List<string> modelNames)
        {
            return await _context.Forecast
                .Include(f => f.WeatherModel)  // Include navigation property
                .Include(f => f.Location)      // Include navigation property
                .Where(f => f.LocationId == locationId &&
                           modelNames.Contains(f.WeatherModel.Name))  // FIXED: Use navigation property
                .OrderBy(f => f.ValidDate)
                .ThenBy(f => f.WeatherModel.Name)  // FIXED: Use navigation property instead of f.Model
                .ToListAsync();
        }

        public async Task<List<Forecast>> GetForecastByLocationAndModelIds(int locationId, List<int> modelIds)
        {
            return await _context.Forecast
                .Include(f => f.WeatherModel)
                .Include(f => f.Location)
                .Where(f => f.LocationId == locationId && modelIds.Contains(f.WeatherModelId))
                .OrderBy(f => f.ValidDate)
                .ThenBy(f => f.WeatherModelId)
                .ToListAsync();
        }

        public async Task SaveForecastAsync(List<Forecast> forecasts)
        {
            _context.Forecast.AddRange(forecasts);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasRecentForecastAsync(int locationId, DateTime cutoffTime)
        {
            return await _context.Forecast
                .AnyAsync(f => f.LocationId == locationId && f.FetchDate >= cutoffTime);
        }

        public async Task<List<Forecast>> GetForecastsByDateRange(int locationId, DateTime startDate, DateTime endDate)
        {
            return await _context.Forecast
                .Include(f => f.WeatherModel)
                .Include(f => f.Location)
                .Where(f => f.LocationId == locationId &&
                           f.ValidDate >= startDate &&
                           f.ValidDate <= endDate)
                .OrderBy(f => f.ValidDate)
                .ThenBy(f => f.WeatherModel.Name)
                .ToListAsync();
        }

        public async Task<Forecast?> GetLatestForecastAsync(int locationId, int weatherModelId)
        {
            return await _context.Forecast
                .Include(f => f.WeatherModel)
                .Include(f => f.Location)
                .Where(f => f.LocationId == locationId && f.WeatherModelId == weatherModelId)
                .OrderByDescending(f => f.FetchDate)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteOldForecastsAsync(DateTime cutoffDate)
        {
            var oldForecasts = await _context.Forecast
                .Where(f => f.FetchDate < cutoffDate)
                .ToListAsync();

            if (oldForecasts.Any())
            {
                _context.Forecast.RemoveRange(oldForecasts);
                await _context.SaveChangesAsync();
            }
        }
    }
}