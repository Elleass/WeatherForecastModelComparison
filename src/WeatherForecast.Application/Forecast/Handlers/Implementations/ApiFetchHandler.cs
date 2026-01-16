using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;

namespace WeatherForecast.Application.Forecast.Handlers.Implementations
{
    /// <summary>
    /// KROK 4: Pobierz prognozy z zewnętrznego API
    /// Tylko jeśli cache był pusty (context. Forecasts == null)
    /// </summary>
    public class ApiFetchHandler : BaseForecastHandler
    {
        private readonly IWeatherApiClient _apiClient;

        public ApiFetchHandler(
            IWeatherApiClient apiClient,
            ILogger<ApiFetchHandler> logger) : base(logger)
        {
            _apiClient = apiClient;
        }

        protected override async Task ProcessAsync(ForecastContext context)
        {
            // Jeśli już mamy prognozy z cache - pomiń API
            if (context.Forecasts != null && context.Forecasts.Any())
            {
                Logger.LogInformation("Forecasts already loaded from cache - skipping API call");
                return;
            }

            if (!context.Latitude.HasValue || !context.Longitude.HasValue || !context.LocationId.HasValue)
            {
                context.ErrorMessage = "Missing coordinates or locationId for API call";
                return;
            }

            Logger.LogInformation("Fetching forecasts from API for coordinates ({lat}, {lng})",
                context.Latitude, context.Longitude);

            try
            {
                var forecasts = await _apiClient.FetchForecastsAsync(
                    context.Latitude.Value,
                    context.Longitude.Value,
                    context.LocationId.Value);

                if (forecasts == null || !forecasts.Any())
                {
                    context.ErrorMessage = "No forecasts returned from API";
                    return;
                }

                context.Forecasts = forecasts;
                context.FromCache = false;

                Logger.LogInformation("Successfully fetched {count} forecasts from API", forecasts.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to fetch forecasts from API");
                context.ErrorMessage = $"API error: {ex.Message}";
            }
        }
    }
}