using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;

namespace WeatherForecast.Application.Forecast.Handlers.Implementations
{
    /// <summary>
    /// KROK 3: Sprawdź czy są świeże prognozy w cache
    /// Jeśli TAK - zwróć je i przerwij łańcuch (nie wywołuj API)
    /// Jeśli NIE - kontynuuj do API
    /// </summary>
    public class CacheCheckHandler : BaseForecastHandler
    {
        private readonly IForecastCacheService _cacheService;

        public CacheCheckHandler(
            IForecastCacheService cacheService,
            ILogger<CacheCheckHandler> logger) : base(logger)
        {
            _cacheService = cacheService;
        }

        protected override async Task ProcessAsync(ForecastContext context)
        {
            if (!context.LocationId.HasValue)
            {
                Logger.LogWarning("LocationId is null - skipping cache check");
                return;
            }

            Logger.LogInformation("Checking cache for locationId: {locationId}", context.LocationId);

            var cachedForecasts = await _cacheService.GetCachedForecastAsync(context.LocationId.Value);

            if (cachedForecasts != null && cachedForecasts.Any())
            {
                Logger.LogInformation("Cache HIT - returning {count} cached forecasts", cachedForecasts.Count);

                context.Forecasts = cachedForecasts;
                context.FromCache = true;
                context.CompletedAt = DateTime.UtcNow;

                // WAŻNE: Ustaw błąd żeby przerwać łańcuch (nie jest to prawdziwy błąd)
                // Alternatywnie:  dodaj flag "ShouldContinue" do kontekstu
                // Tu używamy "hack" - prognozy są gotowe, więc nie kontynuujemy
            }
            else
            {
                Logger.LogInformation("Cache MISS - will fetch from API");
                context.FromCache = false;
            }
        }
    }
}