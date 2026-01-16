using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;

namespace WeatherForecast.Application.Forecast.Handlers.Implementations
{
    /// <summary>
    /// KROK 5: Zapisz prognozy do bazy danych (cache)
    /// Tylko jeśli zostały pobrane z API (FromCache = false)
    /// </summary>
    public class CacheSaveHandler : BaseForecastHandler
    {
        private readonly IForecastRepository _repository;

        public CacheSaveHandler(
            IForecastRepository repository,
            ILogger<CacheSaveHandler> logger) : base(logger)
        {
            _repository = repository;
        }

        protected override async Task ProcessAsync(ForecastContext context)
        {
            // Tylko zapisuj jeśli dane są z API (nie z cache)
            if (context.FromCache)
            {
                Logger.LogInformation("Forecasts from cache - skipping save");
                return;
            }

            if (context.Forecasts == null || !context.Forecasts.Any())
            {
                Logger.LogWarning("No forecasts to save");
                return;
            }

            Logger.LogInformation("Saving {count} forecasts to database", context.Forecasts.Count);

            try
            {
                await _repository.SaveForecastAsync(context.Forecasts);
                Logger.LogInformation("Successfully saved forecasts to cache");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save forecasts to cache");
                // NIE ustawiamy ErrorMessage - to nie jest krytyczny błąd
                // Prognozy już są w pamięci, więc możemy je zwrócić
            }
        }
    }
}