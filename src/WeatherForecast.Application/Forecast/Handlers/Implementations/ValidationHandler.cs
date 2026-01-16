using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Forecast.Handlers.Implementations
{
    /// <summary>
    /// KROK 1: Walidacja nazwy miasta
    /// Sprawdza czy city nie jest null/empty/whitespace
    /// </summary>
    public class ValidationHandler : BaseForecastHandler
    {
        public ValidationHandler(ILogger<ValidationHandler> logger) : base(logger)
        {
        }

        protected override Task ProcessAsync(ForecastContext context)
        {
            Logger.LogInformation("Validating city name: {city}", context.City);

            // Walidacja 1: Nie może być puste
            if (string.IsNullOrWhiteSpace(context.City))
            {
                context.ErrorMessage = "City name is required";
                return Task.CompletedTask;
            }

            // Walidacja 2: Trim whitespace
            context.City = context.City.Trim();

            // Walidacja 3: Min/max długość (opcjonalne)
            if (context.City.Length < 2)
            {
                context.ErrorMessage = "City name must be at least 2 characters long";
                return Task.CompletedTask;
            }

            if (context.City.Length > 100)
            {
                context.ErrorMessage = "City name cannot exceed 100 characters";
                return Task.CompletedTask;
            }

            Logger.LogInformation("Validation passed for city: {city}", context.City);
            return Task.CompletedTask;
        }
    }
}