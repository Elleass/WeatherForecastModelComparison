using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Infrastructure.Persistence;

namespace WeatherForecast.Infrastructure.ExternalApis.OpenMeteo
{
    public class OpenMeteoGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherForecastContext _context;
        private readonly ILogger<OpenMeteoGeocodingService> _logger;

        public OpenMeteoGeocodingService(
            HttpClient httpClient,
            WeatherForecastContext context,
            ILogger<OpenMeteoGeocodingService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        public async Task<(double lat, double lng)?> GetCoordinatesAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogWarning("GetCoordinatesAsync called with empty city name");
                return null;
            }

            city = city.Trim();
            var originalCity = city;  // Zachowaj oryginalną nazwę
            var normalizedCity = NormalizeCity(city);  // Dla API użyj znormalizowanej

            // KROK 1: Sprawdź CACHE (używaj oryginalnej nazwy!)
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Name == originalCity);

            if (location != null)
            {
                _logger.LogInformation("Cache HIT for city: {city}", originalCity);
                return (Math.Round(location.Latitude, 2), Math.Round(location.Longitude, 2));
            }

            // KROK 2: CACHE MISS - wywołaj External API (użyj normalized)
            _logger.LogInformation("Cache MISS - fetching geocoding for:  {city} (normalized: {normalized})",
                originalCity, normalizedCity);

            OpenMeteoGeoResponse? response;

            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["name"] = normalizedCity,  // ← Użyj normalized! 
                    ["count"] = "1",
                    ["language"] = "en",
                    ["format"] = "json"
                };

                var queryString = string.Join("&",
                    queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                var requestUri = $"https://geocoding-api.open-meteo.com/v1/search?{queryString}";
                var uri = new Uri(requestUri);

                _logger.LogDebug("Geocoding API URL: {url}", requestUri);

                var httpResponse = await _httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();

                response = await httpResponse.Content.ReadFromJsonAsync<OpenMeteoGeoResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch geocoding for city: {city}", originalCity);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geocoding for city: {city}", originalCity);
                return null;
            }

            // KROK 3: Parsowanie + zapis (zapisz ORYGINALNĄ nazwę!)
            if (response?.Results?.Count > 0)
            {
                var result = response.Results[0];

                var newLocation = new Location
                {
                    Name = originalCity,  // ← Oryginalna nazwa w bazie! 
                    Latitude = result.Latitude,
                    Longitude = result.Longitude
                };

                _context.Locations.Add(newLocation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved location {city} to database", originalCity);

                return (Math.Round(result.Latitude, 2), Math.Round(result.Longitude, 2));
            }

            _logger.LogWarning("City not found: {city}", originalCity);
            return null;
        }

        private class OpenMeteoGeoResponse
        {
            public List<OpenMeteoGeoResult>? Results { get; set; }
        }

        private class OpenMeteoGeoResult
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private string NormalizeCity(string city)
        {
            // Podstawowe mapowanie polskich znaków
            var normalized = city
                .Replace("ą", "a")
                .Replace("ć", "c")
                .Replace("ę", "e")
                .Replace("ł", "l")
                .Replace("ń", "n")
                .Replace("ó", "o")
                .Replace("ś", "s")
                .Replace("ź", "z")
                .Replace("ż", "z")
                .Replace("Ą", "A")
                .Replace("Ć", "C")
                .Replace("Ę", "E")
                .Replace("Ł", "L")
                .Replace("Ń", "N")
                .Replace("Ó", "O")
                .Replace("Ś", "S")
                .Replace("Ź", "Z")
                .Replace("Ż", "Z");

            return normalized;
        }
    }
}