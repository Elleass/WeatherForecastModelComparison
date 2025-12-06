
using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;

namespace WeatherForecastv2.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lng)> GetCoordinatesAsync(string city);
    }

    public class GeoService : IGeocodingService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient; //learn more about this later
        private readonly WeatherForecastContext _context;
        public GeoService(WeatherForecastContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<(double lat, double lng)> GetCoordinatesAsync(string city)
        {
            // Ensure city is not null/empty
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City must be provided", nameof(city));

            var location = await _context.Location
                .FirstOrDefaultAsync(b => b.Name == city);

            if (location == null)
            {
                // encode city name
                var encodedCity = Uri.EscapeDataString(city);

                // Open-Meteo geocoding API (no API key)
                string baseUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={encodedCity}&count=1&language=en&format=json";

                var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(baseUrl);

                if (response?.Results?.Count > 0)
                {
                    var r = response.Results[0];

                    var newLocation = new WeatherForecastv2.Models.Location
                    {
                        Name = city,
                        Latitude = r.Latitude,
                        Longitude = r.Longitude
                    };

                    _context.Location.Add(newLocation);
                    await _context.SaveChangesAsync();

                    return (r.Latitude, r.Longitude);
                }

                throw new Exception($"Coordinates not found for the given city: {city}");
            }
            return (location.Latitude, location.Longitude);
        }

        // Models for the Open-Meteo response
        public class OpenMeteoResponse
        {
            public List<OpenMeteoResult> ?Results { get; set; }
        }

        public class OpenMeteoResult
        {
            // Match the JSON: "latitude" and "longitude"
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}