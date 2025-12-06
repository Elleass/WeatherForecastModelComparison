using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;
using WeatherForecastv2.Models;


namespace WeatherForecastv2.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        public readonly WeatherForecastContext _context;

        public LocationRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        public async Task<Location?> GetByCityNameAsync(string cityName)
        {
            return await _context.Location
                .FirstOrDefaultAsync(l => l.Name == cityName);
        }

    }
}
