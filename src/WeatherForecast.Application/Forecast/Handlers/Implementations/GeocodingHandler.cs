using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;

namespace WeatherForecast.Application.Forecast.Handlers.Implementations
{
    /// <summary>
    /// KROK 2: Geokodowanie - konwertuje miasto na współrzędne
    /// Używa IGeocodingService (który używa cache + OpenMeteo API)
    /// </summary>
    public class GeocodingHandler : BaseForecastHandler
    {
        private readonly IGeocodingService _geocodingService;
        private readonly ILocationRepository _locationRepository;

        public GeocodingHandler(
            IGeocodingService geocodingService,
            ILocationRepository locationRepository,
            ILogger<GeocodingHandler> logger) : base(logger)
        {
            _geocodingService = geocodingService;
            _locationRepository = locationRepository;
        }

        protected override async Task ProcessAsync(ForecastContext context)
        {
            Logger.LogInformation("Geocoding city: {city}", context.City);

            // Pobierz współrzędne (z cache lub API)
            var coordinates = await _geocodingService.GetCoordinatesAsync(context.City);

            if (coordinates == null)
            {
                context.ErrorMessage = $"City '{context.City}' not found";
                return;
            }

            context.Latitude = coordinates.Value.lat;
            context.Longitude = coordinates.Value.lng;

            Logger.LogInformation("Coordinates found: ({lat}, {lng})",
                context.Latitude, context.Longitude);

            // Pobierz LocationId z bazy (potrzebne do zapisu prognoz)
            var location = await _locationRepository.GetByCityNameAsync(context.City);

            if (location != null)
            {
                context.LocationId = location.Id;
                Logger.LogInformation("LocationId: {locationId}", context.LocationId);
            }
            else
            {
                context.ErrorMessage = "Failed to retrieve LocationId from database";
            }
        }
    }
}