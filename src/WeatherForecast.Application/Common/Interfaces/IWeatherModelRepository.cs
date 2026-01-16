using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface IWeatherModelRepository
    {
        Task<List<WeatherModel>> GetModelsAsync();
        Task<WeatherModel?> GetByIdAsync(int id);
        Task<WeatherModel?> GetByNameAsync(string name);
    }
}