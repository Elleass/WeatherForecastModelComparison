using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface IForecastRepository
    {
        Task<List<Domain.Entities.Forecast>> GetRecentForecastAsync(int locationId, DateTime cutoffTime);
        Task<bool> HasRecentForecastAsync(int locationId, DateTime cutoffTime);
        Task SaveForecastAsync(List<Domain.Entities.Forecast> forecasts);
        Task DeleteOldForecastsAsync(DateTime cutoffDate);
        Task<Domain.Entities.Forecast?> GetLatestForecastAsync(int locationId, int weatherModelId);
    }
}