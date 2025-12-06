namespace WeatherForecastv2.Models
{
    public class WeatherModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Provider { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();
    }
}