using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Infrastructure.Caching
{
    /// <summary>
    /// Serwis cache'owania prognoz pogodowych
    /// U¿ywa bazy danych jako warstwy cache (zamiast Redis/Memory)
    /// </summary>
    public class ForecastCacheService : IForecastCacheService
    {
        private readonly IForecastRepository _repository;
        private readonly ILogger<ForecastCacheService> _logger;

        // Konfiguracja:  jak d³ugo cache jest wa¿ny
        // 6 godzin = prognozy s¹ aktualne przez ten czas
        public TimeSpan CacheValidityPeriod { get; } = TimeSpan.FromHours(6);

        public ForecastCacheService(
            IForecastRepository repository,
            ILogger<ForecastCacheService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera prognozy z cache (jeœli s¹ œwie¿e)
        /// </summary>
        /// <param name="locationId">ID lokalizacji</param>
        /// <returns>Lista prognoz lub null jeœli cache niewa¿ny</returns>
        public async Task<List<Forecast>?> GetCachedForecastAsync(int locationId)
        {
            // Oblicz czas cutoff (6 godzin wstecz)
            // Akceptujemy tylko prognozy pobrane po 08:00
            var cutoffTime = DateTime.UtcNow.Subtract(CacheValidityPeriod);

            _logger.LogDebug("Checking cache for locationId={LocationId}, cutoff={Cutoff}",
                locationId, cutoffTime);

            // SprawdŸ czy s¹ œwie¿e prognozy
            var hasRecent = await _repository.HasRecentForecastAsync(locationId, cutoffTime);

            if (!hasRecent)
            {
                _logger.LogInformation("Cache MISS for locationId={LocationId}", locationId);
                return null;  // Brak œwie¿ych danych trzeba pobraæ z API
            }

            // Cache HIT - pobierz prognozy
            _logger.LogInformation("Cache HIT for locationId={LocationId}", locationId);

            var forecasts = await _repository.GetRecentForecastAsync(locationId, cutoffTime);

            _logger.LogInformation("Returned {Count} cached forecasts", forecasts.Count);

            return forecasts;
        }

        /// <summary>
        /// Czyœci stare prognozy dla danej lokalizacji
        /// (opcjonalne - mo¿na wywo³aæ przez cleanup job)
        /// </summary>
        public async Task InvalidateCacheAsync(int locationId)
        {
            _logger.LogInformation("Invalidating cache for locationId={LocationId}", locationId);

            // Usuñ wszystkie prognozy (wymusza pobranie nowych)
            await _repository.DeleteOldForecastsAsync(DateTime.UtcNow);
        }

   
        public async Task CleanupOldForecastsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);  // Usuñ starsze ni¿ 7 dni

            _logger.LogInformation("Cleaning up forecasts older than {Date}", cutoffDate);

            await _repository.DeleteOldForecastsAsync(cutoffDate);
        }
    }
}