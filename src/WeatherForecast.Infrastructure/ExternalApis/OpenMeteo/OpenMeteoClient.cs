using Microsoft.Extensions.Logging;
using System.Globalization;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Infrastructure.Persistence.Repositories;

namespace WeatherForecast.Infrastructure.ExternalApis.OpenMeteo
{
    /// <summary>
    /// Klient HTTP do API Open-Meteo
    /// Odpowiedzialny za budowanie URL i wykonywanie requestów
    /// </summary>
    public class OpenMeteoClient : IWeatherApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IWeatherModelRepository _modelRepository;
        private readonly IOpenMeteoParser _parser;
        private readonly ILogger<OpenMeteoClient> _logger;

        public OpenMeteoClient(
            HttpClient httpClient,
            IWeatherModelRepository modelRepository,
            IOpenMeteoParser parser,
            ILogger<OpenMeteoClient> logger)
        {
            _httpClient = httpClient;
            _modelRepository = modelRepository;
            _parser = parser;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera prognozy dla wszystkich aktywnych modeli pogodowych
        /// </summary>
        /// <param name="lat">Szerokość geograficzna</param>
        /// <param name="lng">Długość geograficzna</param>
        /// <param name="locationId">ID lokalizacji w bazie danych</param>
        /// <returns>Lista prognoz ze wszystkich modeli</returns>
        public async Task<List<Forecast>> FetchForecastsAsync(double lat, double lng, int locationId)
        {
            // Pobierz listę aktywnych modeli z bazy
            var models = await _modelRepository.GetModelsAsync();

            if (!models.Any())
            {
                _logger.LogWarning("No active weather models found in database");
                return new List<Forecast>();
            }

            _logger.LogInformation("Fetching forecasts for {Count} models", models.Count);

            var allForecasts = new List<Forecast>();

            // Iteruj po każdym modelu i pobierz dane
            foreach (var model in models.Where(m => m.IsActive && !string.IsNullOrWhiteSpace(m.Name)))
            {
                try
                {
                    // Buduj URL dla tego modelu
                    var url = BuildUrl(lat, lng, model.Name!);

                    _logger.LogInformation(" Calling API for model: {Model}", model.Name);
                    _logger.LogDebug("URL: {Url}", url);

                    // Wykonaj HTTP GET
                    var response = await _httpClient.GetStringAsync(url);

                    // Parsuj JSON → List<Forecast>
                    var forecasts = _parser.Parse(response, locationId, model.Id);

                    allForecasts.AddRange(forecasts);

                    _logger.LogInformation("Fetched {Count} forecasts for model {Model}",
                        forecasts.Count, model.Name);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP error fetching forecast for model: {Model}", model.Name);
                    // Kontynuuj z innymi modelami (nie przerywaj całości)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error fetching forecast for model: {Model}", model.Name);
                }
            }

            _logger.LogInformation("Total forecasts fetched: {Count}", allForecasts.Count);
            return allForecasts;
        }

        /// <summary>
        /// Buduje URL do API Open-Meteo
        /// </summary>
        private string BuildUrl(double lat, double lng, string modelName)
        {
            // WAŻNE: używamy InvariantCulture żeby kropka była separatorem dziesiętnym
            // Polskie locale używa przecinka:  52,23 (źle!)
            // InvariantCulture używa kropki: 52.23 (dobrze!)
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);

            // Parametry API: 
            // - latitude, longitude - współrzędne
            // - hourly - jakie dane chcemy (temperatura, opady, etc.)
            // - models - który model pogodowy
            // - timezone=auto - automatyczna strefa czasowa
            // - forecast_days=3 - 3 dni prognozy (72 godziny)
            return $"https://api.open-meteo.com/v1/forecast?" +
                   $"latitude={latStr}&longitude={lngStr}" +
                   "&hourly=temperature_2m,apparent_temperature,precipitation,precipitation_probability," +
                   "wind_speed_10m,relative_humidity_2m,surface_pressure,cloud_cover,visibility,uv_index" +
                   $"&models={modelName}&timezone=auto&forecast_days=3";
        }
    }
}