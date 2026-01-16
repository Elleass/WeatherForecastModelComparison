namespace WeatherForecast.Application.Forecast.DTOs
{
    public class ForecastDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int WeatherModelId { get; set; }
        public string? ModelName { get; set; }

        public DateTime FetchDate { get; set; }
        public DateTime ValidDate { get; set; }

        public double Temperature2m { get; set; }
        public double ApparentTemperature { get; set; }
        public double Precipitation { get; set; }
        public string? PrecipitationType { get; set; }
        public int PrecipitationProbability { get; set; }
        public double WindSpeed10m { get; set; }
        public double Humidity2m { get; set; }
        public double PressureSurface { get; set; }
        public int CloudCover { get; set; }
        public double Visibility { get; set; }
        public int UvIndex { get; set; }
    }
}