using Microsoft.AspNetCore.Mvc;
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


        [HttpGet("{city}")]
        public async Task<IActionResult> GetCityBy(string city)
        {
            var (lat, lng) = await _geo.GetCoordinatesAsync(city);

            return Ok(new CoordinatesDto
            {
                Lat = lat,
                Lng = lng
            });
        }
        public class CoordinatesDto
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
