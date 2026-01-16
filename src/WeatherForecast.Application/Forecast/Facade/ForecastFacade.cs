using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Forecast.Handlers;
using WeatherForecast.Application.Forecast.Handlers.Implementations;

namespace WeatherForecast.Application.Forecast.Facade
{
    /// <summary>
    /// WZORZEC FASADA
    /// Upraszcza dostęp do złożonego systemu handlerów
    /// Kontroler widzi tylko:   GetForecastAsync(city) → ForecastResult
    /// </summary>
    public class ForecastFacade : IForecastFacade
    {
        private readonly IForecastHandler _handlerChain;
        private readonly ILogger<ForecastFacade> _logger;

        /// <summary>
        /// Konstruktor - buduje łańcuch handlerów
        /// DI automatycznie wstrzykuje wszystkie zależności
        /// </summary>
        public ForecastFacade(
            ValidationHandler validationHandler,
            GeocodingHandler geocodingHandler,
            CacheCheckHandler cacheCheckHandler,
            ApiFetchHandler apiFetchHandler,
            CacheSaveHandler cacheSaveHandler,
            ILogger<ForecastFacade> logger)
        {
            _logger = logger;

            // ═══════════════════════════════════════════════════════
            // BUDOWANIE ŁAŃCUCHA (Chain of Responsibility)
            // ═══════════════════════════════════════════════════════
            // Kolejność ma znaczenie!   
            // 1. Validation → 2. Geocoding → 3. Cache → 4. API → 5. Save

            _handlerChain = validationHandler;

            validationHandler
                .SetNext(geocodingHandler)
                .SetNext(cacheCheckHandler)
                .SetNext(apiFetchHandler)
                .SetNext(cacheSaveHandler);

            _logger.LogInformation("ForecastFacade initialized with handler chain");
        }

        /// <summary>
        /// Główna metoda - pobiera prognozy dla miasta
        /// </summary>
        public async Task<ForecastResult> GetForecastAsync(string city)
        {
            _logger.LogInformation("GetForecastAsync called for city: {city}", city);

            // ═══════════════════════════════════════════════════════
            // 1. Utwórz kontekst
            // ═══════════════════════════════════════════════════════
            var context = new ForecastContext
            {
                City = city,
                StartedAt = DateTime.UtcNow
            };

            // ═══════════════════════════════════════════════════════
            // 2. Uruchom łańcuch handlerów
            // ═══════════════════════════════════════════════════════
            var result = await _handlerChain.HandleAsync(context);

            // ═══════════════════════════════════════════════════════
            // 3. Konwertuj ForecastContext → ForecastResult
            // ═══════════════════════════════════════════════════════
            if (result.HasError)
            {
                _logger.LogWarning("Forecast request failed: {error}", result.ErrorMessage);
                return ForecastResult.Failure(result.ErrorMessage!);
            }

            if (result.Forecasts == null || !result.Forecasts.Any())
            {
                _logger.LogWarning("No forecasts found for city: {city}", city);
                return ForecastResult.Failure("No forecasts available");
            }

            // ═══════════════════════════════════════════════════════
            // 4. Zwróć wynik z metadata
            // ═══════════════════════════════════════════════════════
            _logger.LogInformation(
                "Successfully retrieved {count} forecasts for {city} (fromCache: {fromCache}, duration: {duration}ms)",
                result.Forecasts.Count,
                city,
                result.FromCache,
                result.Duration?.TotalMilliseconds ?? 0);

            return result.FromCache
                ? ForecastResult.SuccessFromCache(result.Forecasts)
                : ForecastResult.SuccessFromApi(result.Forecasts);
        }
    }
}