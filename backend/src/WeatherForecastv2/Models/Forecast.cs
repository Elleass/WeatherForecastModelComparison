using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WeatherForecastv2.Models
{
    public class Forecast
    {
        public int Id { get; set; }

        // Location relationship
        public int LocationId { get; set; }
        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime FetchDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ValidDate { get; set; }

        // WeatherModel relationship - FIXED
        public int WeatherModelId { get; set; }  // Changed from WeatherModelName
        [ForeignKey(nameof(WeatherModelId))]
        [JsonIgnore]
        public WeatherModel WeatherModel { get; set; } = null!;

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