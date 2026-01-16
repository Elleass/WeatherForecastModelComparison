using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecast.Application.Forecast.Facade;

namespace WeatherForecast.Web.Controllers
{
    /// <summary>
    /// API Controller dla prognoz pogodowych
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("forecast")]  // Rate limit:  10 req/min
    public class ForecastController : ControllerBase
    {
        private readonly IForecastFacade _forecastFacade;
        private readonly ILogger<ForecastController> _logger;

        public ForecastController(
            IForecastFacade forecastFacade,
            ILogger<ForecastController> logger)
        {
            _forecastFacade = forecastFacade;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera prognozy pogody dla miasta
        /// </summary>
        /// <param name="city">Nazwa miasta (np. "Warsaw", "London")</param>
        /// <returns>Lista prognoz dla różnych modeli pogodowych</returns>
        /// <response code="200">Zwraca prognozy</response>
        /// <response code="400">Nieprawidłowa nazwa miasta</response>
        /// <response code="404">Miasto nie znalezione</response>
        /// <response code="429">Za dużo requestów (rate limit)</response>
        [HttpGet("{city}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetForecast(string city)
        {
            _logger.LogInformation("GET /api/forecast/{city} called", city);

            // Deleguj całą logikę do Facade
            var result = await _forecastFacade.GetForecastAsync(city);

            // Mapuj ForecastResult → HTTP response
            if (!result.Success)
            {
                _logger.LogWarning("Forecast request failed: {error}", result.ErrorMessage);

                // Rozróżnij typ błędu
                if (result.ErrorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return NotFound(new { error = result.ErrorMessage });
                }

                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Returning {count} forecasts (fromCache: {fromCache})",
                result.TotalCount, result.FromCache);

            // Zwróć sukces z metadata
            return Ok(new
            {
                success = true,
                data = result.Forecasts,
                metadata = new
                {
                    totalCount = result.TotalCount,
                    fromCache = result.FromCache,
                    fetchedAt = result.FetchedAt
                }
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [EnableRateLimiting("api")]  // Luźniejszy limit dla health check
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}