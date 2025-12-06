using WeatherForecastv2.Models;

namespace WeatherForecastv2.Repositories
{
    public interface IForecastRepository
    {
        Task<List<Forecast>> GetRecentForecastAsync(int locationId, DateTime cutoffTime);
        Task<List<Forecast>> GetForecastByLocationAndModels(int locationId, List<string> modelNames);
        Task<List<Forecast>> GetForecastByLocationAndModelIds(int locationId, List<int> modelIds);
        Task<List<Forecast>> GetForecastsByDateRange(int locationId, DateTime startDate, DateTime endDate);
        Task<Forecast?> GetLatestForecastAsync(int locationId, int weatherModelId);
        Task SaveForecastAsync(List<Forecast> forecasts);
        Task<bool> HasRecentForecastAsync(int locationId, DateTime cutoffTime);
        Task DeleteOldForecastsAsync(DateTime cutoffDate);
    }
}