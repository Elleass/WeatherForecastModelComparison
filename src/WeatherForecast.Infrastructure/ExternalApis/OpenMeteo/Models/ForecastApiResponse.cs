using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherForecast.Infrastructure.ExternalApis.OpenMeteo.Models
{
    /// <summary>
    /// G³ówna odpowiedŸ z API Open-Meteo
    /// Zawiera sekcjê "hourly" z danymi godzinowymi
    /// </summary>
    public class ForecastApiResponse
    {
        public HourlyData? Hourly { get; set; }
    }

    /// <summary>
    /// Dane godzinowe z prognozy
    /// Time - lista timestampów
    /// AdditionalData - wszystkie inne pola (temperature_2m, precipitation, etc.)
    /// </summary>
    public class HourlyData
    {
        // Lista timestampów:  ["2026-01-16T00:00", "2026-01-16T01:00", ...]
        public List<string> Time { get; set; } = new();

        // [JsonExtensionData] - automatycznie przechwytuje WSZYSTKIE inne pola JSON
        // Dziêki temu nie musimy definiowaæ 20+ properties
        // temperature_2m, precipitation, wind_speed, etc.  wszystko l¹duje tutaj
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
    }
}