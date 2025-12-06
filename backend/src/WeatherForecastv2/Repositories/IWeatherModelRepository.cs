using WeatherForecastv2.Models;


namespace WeatherForecastv2.Repositories
{
    public interface IWeatherModelRepository
    {
        Task<List<WeatherModel>> GetModelsAsync();
        Task<WeatherModel?> GetByIdAsync(int id);
        Task<WeatherModel?> GetByNameAsync(string name);
    }
}
