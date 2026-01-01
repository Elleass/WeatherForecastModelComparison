
using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;

namespace WeatherForecastv2.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lng)?> GetCoordinatesAsync(string city);
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

        public async Task<(double lat, double lng)?> GetCoordinatesAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return null;

            city = city.Trim();

            // 1️⃣ Check DB
            var location = await _context.Location
                .FirstOrDefaultAsync(l => l.Name == city);

            if (location != null)
            {
                return (
                    Math.Round(location.Latitude, 2),
                    Math.Round(location.Longitude, 2)
                );
            }

            // 2️⃣ Fetch from external API
            var encodedCity = Uri.EscapeDataString(city);
            var url =
                $"https://geocoding-api.open-meteo.com/v1/search?name={encodedCity}&count=1&language=en&format=json";

            OpenMeteoResponse? response;

            try
            {
                response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }

            if (response?.Results?.Count > 0)
            {
                var r = response.Results[0];

                var newLocation = new Models.Location
                {
                    Name = city,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude
                };

                _context.Location.Add(newLocation);
                await _context.SaveChangesAsync();

                return (
                    Math.Round(r.Latitude, 2),
                    Math.Round(r.Longitude, 2)
                );
            }

            // 3️⃣ City not found anywhere
            return null;
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