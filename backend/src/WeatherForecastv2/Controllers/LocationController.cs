using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecastv2.Repositories;
using WeatherForecastv2.Services;

namespace WeatherForecastv2.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationController : ControllerBase
    {
        private readonly IGeocodingService _geo;
        private readonly ILocationRepository _locations;
        public LocationController(IGeocodingService geo, ILocationRepository locations)
        {
            _geo = geo;
            _locations = locations;
        }

        [EnableRateLimiting("external")]
        [HttpGet("{city}")]
        public async Task<IActionResult> GetCityBy(string city)
        {
            var coords = await _geo.GetCoordinatesAsync(city);

            if (coords == null)
                return NotFound($"City '{city}' not found");

            return Ok(new CoordinatesDto
            {
                Lat = coords.Value.lat,
                Lng = coords.Value.lng
            });
        }


        public class CoordinatesDto
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
