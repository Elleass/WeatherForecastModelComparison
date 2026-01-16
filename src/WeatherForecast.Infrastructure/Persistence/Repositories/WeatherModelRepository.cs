using Microsoft.EntityFrameworkCore;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository dla operacji na modelach pogodowych
    /// (ECMWF, ICON, GFS, ARPEGE, etc.)
    /// </summary>
    public class WeatherModelRepository : IWeatherModelRepository
    {
        private readonly WeatherForecastContext _context;

        public WeatherModelRepository(WeatherForecastContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pobierz wszystkie AKTYWNE modele pogodowe
        /// Sortowane alfabetycznie po nazwie
        /// </summary>
        public async Task<List<WeatherModel>> GetModelsAsync()
        {
            return await _context.WeatherModels
                .Where(m => m.IsActive)           // Tylko aktywne modele
                .OrderBy(m => m.Name)             // Sortuj A-Z
                .ToListAsync();
        }

        /// <summary>
        /// Znajdź model po ID
        /// Zwraca null jeśli nie znaleziono
        /// </summary>
        public async Task<WeatherModel?> GetByIdAsync(int id)
        {
            return await _context.WeatherModels
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Znajdź model po nazwie (np. "ecmwf_ifs025")
        /// Przydatne do weryfikacji czy model istnieje w bazie
        /// </summary>
        public async Task<WeatherModel?> GetByNameAsync(string name)
        {
            return await _context.WeatherModels
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        /// <summary>
        /// Dodaj nowy model pogodowy
        /// (Opcjonalne - jeśli chcesz dynamicznie dodawać modele)
        /// </summary>
        public async Task<WeatherModel> AddAsync(WeatherModel model)
        {
            _context.WeatherModels.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        /// <summary>
        /// Dezaktywuj model (soft delete)
        /// Zamiast usuwać, ustawiamy IsActive = false
        /// </summary>
        public async Task DeactivateAsync(int id)
        {
            var model = await GetByIdAsync(id);
            if (model != null)
            {
                model.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}