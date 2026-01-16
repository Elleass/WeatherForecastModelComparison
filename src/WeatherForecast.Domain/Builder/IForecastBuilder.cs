namespace WeatherForecast.Domain.Builders
{
    public interface IForecastBuilder
    {
        IForecastBuilder WithLocation(int locationId);
        IForecastBuilder WithModel(int weatherModelId);
        IForecastBuilder WithDateTime(DateTime fetchDate, DateTime validDate);
        IForecastBuilder WithTemperature(double temp, double apparent);
        IForecastBuilder WithPrecipitation(double amount, int probability, string? type = null);
        IForecastBuilder WithWind(double speed);
        IForecastBuilder WithAtmosphere(double humidity, double pressure, int cloudCover, double visibility);
        IForecastBuilder WithUvIndex(int uvIndex);
        Entities.Forecast Build();
        IForecastBuilder Reset();
    }
}