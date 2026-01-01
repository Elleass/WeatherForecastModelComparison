using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecastv2.Services;
[ApiController]
[Route("api/forecast")]
public class ForecastController : ControllerBase
{
    private readonly IFetchForecastService _forecastService;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(
        IFetchForecastService forecastService,
        ILogger<ForecastController> logger)
    {
        _forecastService = forecastService;
        _logger = logger;
    }

    [EnableRateLimiting("external")]
    [HttpGet("{city}")]
    public async Task<IActionResult> GetForecast(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City is required");

        try
        {
            var data = await _forecastService.FetchForecastAsync(city);

            if (data == null)
                return NotFound($"Forecast not found for city '{city}'");

            return Ok(data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "External API failed for city {City}", city);
            return StatusCode(503, "Weather provider unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forecast failed for city {City}", city);
            return StatusCode(500, "Internal forecast error");
        }
    }
}
