using Azure;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherForecastv2.Models;
using WeatherForecastv2.Repositories;
using static WeatherForecastv2.Services.GeoService;

namespace WeatherForecastv2.Services
{
    public interface IFetchForecastService
    {
        Task<List<Forecast>> FetchForecastAsync(string city);
    }

    public class FetchForecast : IFetchForecastService
    {
        private readonly HttpClient _httpClient;
        private readonly IGeocodingService _geoService;
        private readonly ILogger<FetchForecast> _logger;
        private readonly IWeatherModelRepository _modelRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IForecastRepository _forecastRepository;

        //Cache duration - 6 hours ===== TO VERIFY LATER ======
        private readonly TimeSpan _cacheValidityPeriod = TimeSpan.FromHours(6);

        public FetchForecast(
            IGeocodingService geoService,
            HttpClient httpClient,
            IWeatherModelRepository modelRepository,
            ILocationRepository locationRepository,
            IForecastRepository forecastRepository,
            ILogger<FetchForecast> logger)

        {
            _httpClient = httpClient;
            _geoService = geoService;
            _modelRepository = modelRepository;
            _locationRepository = locationRepository;
            _forecastRepository = forecastRepository;
            _logger = logger;

        }

        public async Task<List<Forecast>> FetchForecastAsync(string city)
        {
            _logger.LogInformation($"Fetching forecast for city: {city}");

            //Step 1: Get Coordinates (handles location caching automatically)

            var coords = await _geoService.GetCoordinatesAsync(city);

            if (coords == null)
            {
                return null;
            }
            var (latRaw, lngRaw) = coords.Value; 
            var lat = Math.Round(latRaw, 5);
            var lng = Math.Round(lngRaw, 5);

            //Step 2: Get location from DB (Should exist after geocoding)
            _logger.LogInformation("Geo coords for {City}: lat={Lat}, lng={Lng}", city, lat, lng);

            var location = await _locationRepository.GetByCityNameAsync(city);
            if (location == null)
            {
                throw new InvalidOperationException($"Location should exits after geocdoing the city {city}");
            }
            _logger.LogInformation("DB coords for {City}: id={Id}, lat={Lat}, lng={Lng}",
                city, location.Id, location.Latitude, location.Longitude);

            //Step 3: Check if we have recent forecast (Fetched to 6hours ago)
            var cutoffTime = DateTime.UtcNow.Subtract(_cacheValidityPeriod); //check out later how this work
            var hasRecentForecast = await _forecastRepository.HasRecentForecastAsync(location.Id, cutoffTime);

            if (hasRecentForecast)
            {
                _logger.LogInformation($"Using cached forecast data for location {location.Id}");
                var cached = await _forecastRepository.GetRecentForecastAsync(location.Id, cutoffTime);
                return RoundForecasts(cached);
            }

            //Step 4: Fetch fresh data from openmeteo API
            _logger.LogInformation($"Fetching fresh forecast data from openmeteo API for location: {location.Id}");

            var models = await _modelRepository.GetModelsAsync();
            var freshForecast = await FetchFromApiAsync(lat, lng, location.Id, models);

            //Step 5: Save to database for future caching
            await _forecastRepository.SaveForecastAsync(freshForecast);

            return RoundForecasts(freshForecast);
        }


        private async Task<List<Forecast>> FetchFromApiAsync(double lat, double lng, int locationId, List<WeatherModel> models)
        {
            if (models == null || models.Count == 0)
                throw new InvalidOperationException("No weather models configured.");

            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            var forecasts = new List<Forecast>();
            var fetchTime = DateTime.UtcNow;

            foreach (var model in models)
            {
                var modelName = model.Name?.Trim();
                if (string.IsNullOrWhiteSpace(modelName))
                    continue;

                var fetchUrl =
                    $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lngStr}" +
                    "&hourly=temperature_2m,apparent_temperature,precipitation,precipitation_probability," +
                    "wind_speed_10m,relative_humidity_2m,surface_pressure,cloud_cover,visibility,uv_index" +
                    $"&models={modelName}&timezone=auto&forecast_days=3";

                _logger.LogInformation("Calling Open-Meteo: {Url}", fetchUrl);

                using var response = await _httpClient.GetAsync(fetchUrl, HttpCompletionOption.ResponseHeadersRead);
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Open-Meteo failed. Status: {Status}. Body: {Body}", response.StatusCode, body);
                    throw new HttpRequestException($"Open-Meteo request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
                }

                var payload = JsonSerializer.Deserialize<ForecastApiResponse>(
                    body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (payload?.Hourly == null)
                    throw new Exception("Open-Meteo response missing hourly data.");

                var hourlyData = payload.Hourly;
                var modelData = ExtractModelData(hourlyData); // no suffixes now

                double GetD(List<double> list, int i)
                {
                    if (i < list.Count && double.IsFinite(list[i])) return list[i];
                    return 0; // choose your fallback, or skip the row if you prefer
                }

                int GetI(List<int> list, int i)
                {
                    if (i < list.Count) return list[i];
                    return 0; // fallback
                }

                // Minimal guard: rely on Time + Temperature2m
                int count = Math.Min(hourlyData.Time.Count, modelData.Temperature2m.Count);
                if (count == 0)
                {
                    _logger.LogWarning("Insufficient data to build forecasts for model: {Model}", modelName);
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    forecasts.Add(new Forecast
                    {
                        LocationId = locationId,
                        FetchDate = fetchTime,
                        ValidDate = DateTime.Parse(hourlyData.Time[i]),
                        WeatherModelId = model.Id,
                        Temperature2m = Math.Round(GetD(modelData.Temperature2m, i), 1),
                        ApparentTemperature = Math.Round(GetD(modelData.ApparentTemperature, i), 1),
                        Precipitation = Math.Round(GetD(modelData.Precipitation, i), 1),
                        PrecipitationProbability = GetI(modelData.PrecipitationProbability, i),
                        WindSpeed10m = Math.Round(GetD(modelData.WindSpeed10m, i), 1),
                        Humidity2m = Math.Round(GetD(modelData.RelativeHumidity2m, i), 0),
                        PressureSurface = Math.Round(GetD(modelData.SurfacePressure, i), 1),
                        CloudCover = GetI(modelData.CloudCover, i),
                        Visibility = Math.Round(GetD(modelData.Visibility, i), 1),
                        UvIndex = GetI(modelData.UvIndex, i)
                    });
                }
            }

            return forecasts;
        }

        private List<Forecast> RoundForecasts(List<Forecast> forecasts)
        {
            foreach (var f in forecasts)
            {
                f.Temperature2m = Math.Round(f.Temperature2m, 1);
                f.ApparentTemperature = Math.Round(f.ApparentTemperature, 1);
                f.Precipitation = Math.Round(f.Precipitation, 1);
                f.WindSpeed10m = Math.Round(f.WindSpeed10m, 1);
                f.Humidity2m = Math.Round(f.Humidity2m, 0);
                f.PressureSurface = Math.Round(f.PressureSurface, 1);
                f.Visibility = Math.Round(f.Visibility, 1);

                if (f.Location != null)
                {
                    f.Location.Latitude = Math.Round(f.Location.Latitude, 5);
                    f.Location.Longitude = Math.Round(f.Location.Longitude, 5);
                }
            }
            return forecasts;
        }

        private ModelSpecificData ExtractModelData(HourlyData hourlyData)
        {
            return new ModelSpecificData
            {
                Temperature2m = ExtractDoubleArray(hourlyData, "temperature_2m"),
                ApparentTemperature = ExtractDoubleArray(hourlyData, "apparent_temperature"),
                Precipitation = ExtractDoubleArray(hourlyData, "precipitation"),
                PrecipitationProbability = ExtractIntArray(hourlyData, "precipitation_probability"),
                WindSpeed10m = ExtractDoubleArray(hourlyData, "wind_speed_10m"),
                RelativeHumidity2m = ExtractDoubleArray(hourlyData, "relative_humidity_2m"),
                SurfacePressure = ExtractDoubleArray(hourlyData, "surface_pressure"),
                CloudCover = ExtractIntArray(hourlyData, "cloud_cover"),
                Visibility = ExtractDoubleArray(hourlyData, "visibility"),
                UvIndex = ExtractIntArray(hourlyData, "uv_index")
            };
        }
        private List<double> ExtractDoubleArray(HourlyData hourlyData, string fieldName)
        {
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement) &&
                jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<double>();
                foreach (var el in jsonElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetDouble(out var v))
                        list.Add(v);
                    // skip null/other kinds
                }
                return list;
            }
            return new List<double>();
        }

        private List<int> ExtractIntArray(HourlyData hourlyData, string fieldName)
        {
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement) &&
                jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();
                foreach (var el in jsonElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var v))
                        list.Add(v);
                    // skip null/other kinds
                }
                return list;
            }
            return new List<int>();
        }

        private class ModelSpecificData
        {
            public List<double> Temperature2m { get; set; } = new List<double>();
            public List<double> ApparentTemperature { get; set; } = new List<double>();
            public List<double> Precipitation { get; set; } = new List<double>();
            public List<int> PrecipitationProbability { get; set; } = new List<int>();
            public List<double> WindSpeed10m { get; set;} = new List<double>();
            public List<double> RelativeHumidity2m { get; set; } = new List<double>();
            public List<double> SurfacePressure { get; set; } = new List<double>();
            public List<int> CloudCover { get; set; } = new List<int>();
            public List<double> Visibility { get; set; } = new List<double>();
            public List<int> UvIndex { get; set; } = new List<int>();
        }
    }

    public class ForecastApiResponse
    {
        public HourlyData? Hourly { get; set; }
    }

    public class HourlyData
    {
        public List<string> Time { get; set; } = new List<string>();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }
}