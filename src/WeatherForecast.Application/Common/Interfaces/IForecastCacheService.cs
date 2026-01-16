using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface IForecastCacheService
    {
        TimeSpan CacheValidityPeriod { get; }
        Task<List<Domain.Entities.Forecast>?> GetCachedForecastAsync(int locationId);
        Task InvalidateCacheAsync(int locationId);
    }
}