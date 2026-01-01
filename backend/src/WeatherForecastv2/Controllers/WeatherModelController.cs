using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecastv2.Repositories;
using WeatherForecastv2.Services;

namespace WeatherForecastv2.Controllers
{
    [ApiController]
    [Route("api/weathermodels")]

    public class WeatherModelController : ControllerBase
    {
        private readonly IWeatherModelRepository _weatherModel;



        public WeatherModelController(IWeatherModelRepository weatherModel)
        {

            _weatherModel = weatherModel;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var models = await _weatherModel.GetModelsAsync();
            return Ok(models);
        }
        [EnableRateLimiting("external")]
        [HttpGet("{weatherModelId:int}")]
        public async Task<IActionResult> GetByIdAsync(int weatherModelId)
        {
            var model = await _weatherModel.GetByIdAsync(weatherModelId);
            if (model == null)
                return NotFound();

            return Ok(model);
        }
    }
}