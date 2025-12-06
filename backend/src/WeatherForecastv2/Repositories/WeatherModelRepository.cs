using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;
using WeatherForecastv2.Models;


namespace WeatherForecastv2.Repositories
{
    public class WeatherModelRepository : IWeatherModelRepository
    {
        private readonly WeatherForecastContext _context;

        public WeatherModelRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        public async Task<List<WeatherModel>> GetModelsAsync()
        {
            return await _context.WeatherModel
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<WeatherModel?> GetByIdAsync(int id)
        {
            return await _context.WeatherModel
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<WeatherModel?> GetByNameAsync(string name)
        {
            return await _context.WeatherModel
                .FirstOrDefaultAsync(m => m.Name == name);
        }

    }
}
