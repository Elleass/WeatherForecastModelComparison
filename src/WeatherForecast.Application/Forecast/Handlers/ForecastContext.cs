using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Forecast.Handlers
{
    /// <summary>
    /// Kontekst przekazywany przez łańcuch handlerów
    /// Każdy handler może modyfikować ten obiekt
    /// </summary>
    public class ForecastContext
    {
        // INPUT
        public string City { get; set; } = string.Empty;

        // INTERMEDIATE (wypełniane przez handlery)
        public int? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // OUTPUT
        public List<Domain.Entities.Forecast>? Forecasts { get; set; }
        public bool FromCache { get; set; }

        // ERROR HANDLING
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public string? ErrorMessage { get; set; }

        // METADATA
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public TimeSpan? Duration => CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt
            : null;
    }
}