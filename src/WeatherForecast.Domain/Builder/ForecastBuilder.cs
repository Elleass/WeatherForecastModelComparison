using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Domain.Builders
{
    public class ForecastBuilder : IForecastBuilder
    {
        private Forecast _forecast = new();

        public IForecastBuilder WithLocation(int locationId)
        {
            _forecast.LocationId = locationId;
            return this;
        }

        public IForecastBuilder WithModel(int weatherModelId)
        {
            _forecast.WeatherModelId = weatherModelId;
            return this;
        }

        public IForecastBuilder WithDateTime(DateTime fetchDate, DateTime validDate)
        {
            _forecast.FetchDate = fetchDate;
            _forecast.ValidDate = validDate;
            return this;
        }

        public IForecastBuilder WithTemperature(double temp, double apparent)
        {
            _forecast.Temperature2m = Math.Round(temp, 1);
            _forecast.ApparentTemperature = Math.Round(apparent, 1);
            return this;
        }

        public IForecastBuilder WithPrecipitation(double amount, int probability, string? type = null)
        {
            _forecast.Precipitation = Math.Round(amount, 1);
            _forecast.PrecipitationProbability = probability;
            _forecast.PrecipitationType = type;
            return this;
        }

        public IForecastBuilder WithWind(double speed)
        {
            _forecast.WindSpeed10m = Math.Round(speed, 1);
            return this;
        }

        public IForecastBuilder WithAtmosphere(double humidity, double pressure, int cloudCover, double visibility)
        {
            _forecast.Humidity2m = Math.Round(humidity, 0);
            _forecast.PressureSurface = Math.Round(pressure, 1);
            _forecast.CloudCover = cloudCover;
            _forecast.Visibility = Math.Round(visibility, 1);
            return this;
        }

        public IForecastBuilder WithUvIndex(int uvIndex)
        {
            _forecast.UvIndex = uvIndex;
            return this;
        }

        public Forecast Build()
        {
            if (_forecast.LocationId == 0)
                throw new InvalidOperationException("LocationId is required");
            if (_forecast.WeatherModelId == 0)
                throw new InvalidOperationException("WeatherModelId is required");

            return _forecast;
        }

        public IForecastBuilder Reset()
        {
            _forecast = new Forecast();
            return this;
        }
    }
}