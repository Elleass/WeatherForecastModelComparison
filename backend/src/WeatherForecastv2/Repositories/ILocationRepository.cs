using WeatherForecastv2.Models;


namespace WeatherForecastv2.Repositories
{
    public interface ILocationRepository
    {
        Task<Location?> GetByCityNameAsync(string cityName);
    }
}
