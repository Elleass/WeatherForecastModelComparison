using Azure;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client;
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
            var (lat, lng) = await _geoService.GetCoordinatesAsync(city);


            //Step 2: Get location from DB (Should exist after geocoding) 




            var location = await _locationRepository.GetByCityNameAsync(city);
            if (location == null)
            {
                throw new InvalidOperationException($"Location should exits after geocdoing the city {city}");
            }


            //int locationId = location.Id;


            //Step 3: Check if we have recent forecast (Fetched to 6hours ago)

            var cutoffTime = DateTime.UtcNow.Subtract(_cacheValidityPeriod); //check out later how this work
            var hasRecentForecast = await _forecastRepository.HasRecentForecastAsync(location.Id, cutoffTime);

            if (hasRecentForecast)
            {
                _logger.LogInformation($"Using cached forecast data for location {location.Id}");
                return await _forecastRepository.GetRecentForecastAsync(location.Id, cutoffTime);
            }

            //Step 4: Fetch fresh data from openmeteo API

            _logger.LogInformation($"Fetching fresh forecast data from openmeteo API for location: {location.Id}");

            var models = await _modelRepository.GetModelsAsync();
            var freshForecast = await FetchFromApiAsync(lat, lng, location.Id, models);



            //Step 5: Save to database for future caching


            await _forecastRepository.SaveForecastAsync(freshForecast);

            return freshForecast;

        }


        private async Task<List<Forecast>> FetchFromApiAsync(double lat, double lng, int locationId, List<WeatherModel> models)
        {
            string baseUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lng}";
            string modelsParam = string.Join(",", models.Select(m => m.Name));
            string fetchUrl = baseUrl + $"&hourly=temperature_2m,apparent_temperature,precipitation,precipitation_probability,wind_speed_10m," +
                                        $"relative_humidity_2m,surface_pressure,cloud_cover,visibility,uv_index&" +
                                        $"models={modelsParam}" + "&timezone=auto&forecast_days=3";

            var response = await _httpClient.GetFromJsonAsync<ForecastApiResponse>(fetchUrl);
            if (response?.Hourly == null)
            {
                throw new Exception("Failed to get weather forecast from openmeteo API");
            }

            // Convert API response to your Forecast models
            var forecasts = new List<Forecast>();
            var hourlyData = response.Hourly;
            var fetchTime = DateTime.UtcNow;

            foreach (var model in models)
            {
                var modelName = model.Name; //  e.g., "ecmwf_ifs025", "gfs025"

                var modelData = ExtractModelData(hourlyData, modelName);

                if (modelData.Temperature2m.Count == 0)
                {
                    _logger.LogWarning($"No data found for model: {modelName}");
                    continue;
                }

                for (int i = 0; i < hourlyData.Time.Count && i < modelData.Temperature2m.Count; i++)
                {
                    var forecast = new Forecast
                    {
                        LocationId = locationId,
                        FetchDate = fetchTime,
                        ValidDate = DateTime.Parse(hourlyData.Time[i]),
                        WeatherModelId = model.Id,  // FIXED: Use model.Id instead of modelName
                        Temperature2m = modelData.Temperature2m[i],
                        ApparentTemperature = modelData.ApparentTemperature[i],
                        Precipitation = modelData.Precipitation[i],
                        PrecipitationProbability = modelData.PrecipitationProbability[i],
                        WindSpeed10m = modelData.WindSpeed10m[i],
                        Humidity2m = modelData.RelativeHumidity2m[i],
                        PressureSurface = modelData.SurfacePressure[i],
                        CloudCover = modelData.CloudCover[i],
                        Visibility = modelData.Visibility[i],
                        UvIndex = modelData.UvIndex[i]
                    };

                    forecasts.Add(forecast);
                }
            }

            return forecasts;
        }
        //helper method to extract model-specific data
        private ModelSpecificData ExtractModelData(HourlyData hourlyData, string modelName)
        {
            var modelData = new ModelSpecificData();

            var tempField = $"temperature_2m_{modelName}";
            var apparentTempField = $"apparent_temperature_{modelName}";
            var precipField = $"precipitation_{modelName}";
            var precipProbField = $"precipitation_probability_{modelName}";
            var windField = $"wind_speed_10m_{modelName}";
            var humidityField = $"relative_humidity_2m_{modelName}";
            var pressureField = $"surface_pressure_{modelName}";
            var cloudField = $"cloud_cover_{modelName}";
            var visibilityField = $"visibility_{modelName}";
            var uvField = $"uv_index_{modelName}";


            modelData.Temperature2m = ExtractDoubleArray(hourlyData, tempField);
            modelData.ApparentTemperature = ExtractDoubleArray(hourlyData, apparentTempField);
            modelData.Precipitation = ExtractDoubleArray(hourlyData, precipField);
            modelData.PrecipitationProbability = ExtractIntArray(hourlyData, precipProbField);
            modelData.WindSpeed10m = ExtractDoubleArray(hourlyData, windField);
            modelData.RelativeHumidity2m = ExtractDoubleArray(hourlyData, humidityField);
            modelData.SurfacePressure = ExtractDoubleArray(hourlyData, pressureField);
            modelData.CloudCover = ExtractIntArray(hourlyData, cloudField);
            modelData.Visibility = ExtractDoubleArray(hourlyData, visibilityField);
            modelData.UvIndex = ExtractIntArray(hourlyData, uvField);

            return modelData;


        }

        private List<double> ExtractDoubleArray(HourlyData hourlyData, string fieldName)
        {
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement))
            {
                try
                {
                    return jsonElement.EnumerateArray()
                        .Select(x => x.GetDouble())
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse {FieldName}: {Error}", fieldName, ex.Message);
                }
            }

            return new List<double>();
        }


        private List<int> ExtractIntArray(HourlyData hourlyData, string fieldName)
        {
            if (hourlyData.AdditionalData.TryGetValue(fieldName, out var jsonElement))
            {
                try
                {
                    return jsonElement.EnumerateArray()
                        .Select(x => x.GetInt32())
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse {FieldName}: {Error}", fieldName, ex.Message);
                }
            }

            return new List<int>();
        }

        private class ModelSpecificData
        {
            public List<double> Temperature2m { get; set; } = new List<double>();
            public List<double> ApparentTemperature { get; set; } = new List<double>();
            public List<double> Precipitation { get; set; } = new List<double>();
            public List<int> PrecipitationProbability { get; set; } = new List<int>();
            public List<double> WindSpeed10m { get; set; } = new List<double>();
            public List<double> RelativeHumidity2m { get; set; } = new List<double>();
            public List<double> SurfacePressure { get; set; } = new List<double>();
            public List<int> CloudCover { get; set; } = new List<int>();
            public List<double> Visibility { get; set; } = new List<double>();
            public List<int> UvIndex { get; set; } = new List<int>();
        }
    }



    public class ForecastApiResponse
    {
        public HourlyData Hourly { get; set; }
    }

    public class HourlyData
    {
        public List<string> Time { get; set; } = new List<string>();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();

  
    }
}