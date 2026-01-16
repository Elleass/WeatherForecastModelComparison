using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Common.Interfaces
{
    public interface IOpenMeteoParser
    {
        List<Domain.Entities.Forecast> Parse(string jsonResponse, int locationId, int modelId);
    }
}