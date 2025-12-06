
namespace WeatherForecastv2.Models
{ 
    public class Location
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public ICollection<Models.Forecast>? Forecasts { get; set; }
    }
}