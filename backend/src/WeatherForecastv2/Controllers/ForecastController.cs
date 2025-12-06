using Microsoft.AspNetCore.Mvc;
using WeatherForecastv2.Services;

[ApiController]
[Route("api/forecast")]
public class ForecastController : ControllerBase
{
    private readonly IFetchForecastService _forecastService;

    public ForecastController(IFetchForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetForecast(string city)
    {
        var data = await _forecastService.FetchForecastAsync(city);
        return Ok(data);
    }
}
