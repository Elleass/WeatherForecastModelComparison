using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface ILocationRepository
    {
        Task<Location?> GetByCityNameAsync(string cityName);
        Task<Location> AddAsync(Location location);
    }
}