using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface IWeatherApiClient
    {
        Task<List<Domain.Entities.Forecast>> FetchForecastsAsync(double lat, double lng, int locationId);
    }
}