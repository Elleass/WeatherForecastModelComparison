using WeatherForecast.Application.Forecast.DTOs;

namespace WeatherForecast.Application.Forecast.Facade
{
    public class ForecastResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<ForecastDto>? Forecasts { get; set; }

        public bool FromCache { get; set; }
        public DateTime FetchedAt { get; set; }
        public int TotalCount => Forecasts?.Count ?? 0;

        public static ForecastResult SuccessFromCache(List<Domain.Entities.Forecast> forecasts)
        {
            return new ForecastResult
            {
                Success = true,
                Forecasts = forecasts.Select(f => new ForecastDto
                {
                    Id = f.Id,
                    LocationId = f.LocationId,
                    LocationName = f.Location?.Name,
                    WeatherModelId = f.WeatherModelId,
                    ModelName = f.WeatherModel?.Name,
                    FetchDate = f.FetchDate,
                    ValidDate = f.ValidDate,
                    Temperature2m = f.Temperature2m,
                    ApparentTemperature = f.ApparentTemperature,
                    Precipitation = f.Precipitation,
                    PrecipitationType = f.PrecipitationType,
                    PrecipitationProbability = f.PrecipitationProbability,
                    WindSpeed10m = f.WindSpeed10m,
                    Humidity2m = f.Humidity2m,
                    PressureSurface = f.PressureSurface,
                    CloudCover = f.CloudCover,
                    Visibility = f.Visibility,
                    UvIndex = f.UvIndex
                }).ToList(),
                FromCache = true,
                FetchedAt = DateTime.UtcNow
            };
        }

        public static ForecastResult SuccessFromApi(List<Domain.Entities.Forecast> forecasts)
        {
            return new ForecastResult
            {
                Success = true,
                Forecasts = forecasts.Select(f => new ForecastDto
                {
                    Id = f.Id,
                    LocationId = f.LocationId,
                    LocationName = f.Location?.Name,
                    WeatherModelId = f.WeatherModelId,
                    ModelName = f.WeatherModel?.Name,
                    FetchDate = f.FetchDate,
                    ValidDate = f.ValidDate,
                    Temperature2m = f.Temperature2m,
                    ApparentTemperature = f.ApparentTemperature,
                    Precipitation = f.Precipitation,
                    PrecipitationType = f.PrecipitationType,
                    PrecipitationProbability = f.PrecipitationProbability,
                    WindSpeed10m = f.WindSpeed10m,
                    Humidity2m = f.Humidity2m,
                    PressureSurface = f.PressureSurface,
                    CloudCover = f.CloudCover,
                    Visibility = f.Visibility,
                    UvIndex = f.UvIndex
                }).ToList(),
                FromCache = false,
                FetchedAt = DateTime.UtcNow
            };
        }

        public static ForecastResult Failure(string errorMessage)
        {
            return new ForecastResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Forecasts = null,
                FetchedAt = DateTime.UtcNow
            };
        }
    }
}