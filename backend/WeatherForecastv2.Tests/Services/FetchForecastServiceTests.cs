using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecastv2.Models;
using WeatherForecastv2.Repositories;
using WeatherForecastv2.Services;
using WeatherForecastv2.Tests.TestHelpers;
using Xunit;

namespace WeatherForecastv2.Tests.Services
{
	public class FetchForecastServiceTests
	{
		// Helper: build a fake API response JSON for two models
		private static string BuildFakeApiResponse(IEnumerable<WeatherModel> models)
		{
			// We'll produce hourly.time plus per-model arrays like temperature_2m_{modelName}
			var times = new[] { DateTime.UtcNow.ToString("o"), DateTime.UtcNow.AddHours(1).ToString("o") };

			var hourly = new Dictionary<string, object>
			{
				["time"] = times
			};

			// For each model, add arrays for fields used by service
			foreach (var m in models)
			{
				var suffix = $"_{m.Name}";
				hourly[$"temperature_2m{suffix}"] = new double[] { 1.1, 2.2 };
				hourly[$"apparent_temperature{suffix}"] = new double[] { 0.9, 1.9 };
				hourly[$"precipitation{suffix}"] = new double[] { 0.0, 0.1 };
				hourly[$"precipitation_probability{suffix}"] = new int[] { 10, 20 };
				hourly[$"wind_speed_10m{suffix}"] = new double[] { 3.3, 4.4 };
				hourly[$"relative_humidity_2m{suffix}"] = new double[] { 50.0, 55.0 };
				hourly[$"surface_pressure{suffix}"] = new double[] { 1010.0, 1011.0 };
				hourly[$"cloud_cover{suffix}"] = new int[] { 30, 40 };
				hourly[$"visibility{suffix}"] = new double[] { 10000.0, 11000.0 };
				hourly[$"uv_index{suffix}"] = new int[] { 0, 1 };
			}

			var root = new Dictionary<string, object>
			{
				["hourly"] = hourly
			};

			return JsonSerializer.Serialize(root);
		}

		[Fact]
		public async Task FetchForecastAsync_uses_cache_when_available_and_does_not_call_api()
		{
			var city = "TestCity";

			// mocks
			var geoMock = new Mock<IGeocodingService>();
			geoMock.Setup(g => g.GetCoordinatesAsync(city))
				   .ReturnsAsync((50.0, 20.0));

			var location = new Location { Id = 1, Name = city };
			var locRepo = new Mock<ILocationRepository>();
			locRepo.Setup(r => r.GetByCityNameAsync(city)).ReturnsAsync(location);

			var forecastRepo = new Mock<IForecastRepository>();
			// HasRecentForecastAsync -> true
			forecastRepo.Setup(r => r.HasRecentForecastAsync(1, It.IsAny<DateTime>())).ReturnsAsync(true);
			var cached = new List<Forecast> { new Forecast { Id = 1, LocationId = 1, Temperature2m = 10 } };
			forecastRepo.Setup(r => r.GetRecentForecastAsync(1, It.IsAny<DateTime>())).ReturnsAsync(cached);

			var modelRepo = new Mock<IWeatherModelRepository>();
			modelRepo.Setup(m => m.GetModelsAsync()).ReturnsAsync(new List<WeatherModel> { new WeatherModel { Id = 1, Name = "gfs" } });

			var logger = new Mock<ILogger<FetchForecast>>();

			// HttpClient shouldn't be called — but provide a harmless client
			var httpClient = MockHttpMessageHandler.CreateClient("{}", HttpStatusCode.OK);

			var service = new FetchForecast(
				geoMock.Object,
				httpClient,
				modelRepo.Object,
				locRepo.Object,
				forecastRepo.Object,
				logger.Object
			);

			var result = await service.FetchForecastAsync(city);

			// verify
			result.Should().BeEquivalentTo(cached);
			forecastRepo.Verify(r => r.GetRecentForecastAsync(1, It.IsAny<DateTime>()), Times.Once);
			// Ensure SaveForecastAsync never called because cached
			forecastRepo.Verify(r => r.SaveForecastAsync(It.IsAny<List<Forecast>>()), Times.Never);
		}

		[Fact]
		public async Task FetchForecastAsync_fetches_from_api_when_no_cache_and_saves_results()
		{
			var city = "FreshCity";

			var models = new List<WeatherModel>
			{
				new WeatherModel { Id = 10, Name = "gfs" },
				new WeatherModel { Id = 11, Name = "ecmwf" }
			};

			// Setup mocks
			var geoMock = new Mock<IGeocodingService>();
			geoMock.Setup(g => g.GetCoordinatesAsync(city)).ReturnsAsync((51.1, 17.0));

			var location = new Location { Id = 2, Name = city };
			var locRepo = new Mock<ILocationRepository>();
			locRepo.Setup(r => r.GetByCityNameAsync(city)).ReturnsAsync(location);

			var forecastRepo = new Mock<IForecastRepository>();
			forecastRepo.Setup(r => r.HasRecentForecastAsync(location.Id, It.IsAny<DateTime>())).ReturnsAsync(false);
			forecastRepo.Setup(r => r.SaveForecastAsync(It.IsAny<List<Forecast>>())).Returns(Task.CompletedTask)
						.Verifiable();

			var modelRepo = new Mock<IWeatherModelRepository>();
			modelRepo.Setup(m => m.GetModelsAsync()).ReturnsAsync(models);

			var logger = new Mock<ILogger<FetchForecast>>();

			// Fake API response
			var json = BuildFakeApiResponse(models);
			var httpClient = MockHttpMessageHandler.CreateClient(json, HttpStatusCode.OK);

			var service = new FetchForecast(
				geoMock.Object,
				httpClient,
				modelRepo.Object,
				locRepo.Object,
				forecastRepo.Object,
				logger.Object
			);

			var result = await service.FetchForecastAsync(city);

			// Should save results and return them
			forecastRepo.Verify(r => r.SaveForecastAsync(It.IsAny<List<Forecast>>()), Times.Once);
			result.Should().NotBeNull();
			result.Should().NotBeEmpty();
			// results should reference the model ids that were passed (service uses model.Id for WeatherModelId)
			result.All(f => models.Select(m => m.Id).Contains(f.WeatherModelId)).Should().BeTrue();
		}

		[Fact]
		public async Task FetchForecastAsync_throws_when_location_missing()
		{
			var city = "Nowhere";
			var geoMock = new Mock<IGeocodingService>();
			geoMock.Setup(g => g.GetCoordinatesAsync(city)).ReturnsAsync((0.0, 0.0));

			var locRepo = new Mock<ILocationRepository>();
			locRepo.Setup(r => r.GetByCityNameAsync(city)).ReturnsAsync((Location)null);

			var forecastRepo = new Mock<IForecastRepository>();
			var modelRepo = new Mock<IWeatherModelRepository>();
			modelRepo.Setup(m => m.GetModelsAsync()).ReturnsAsync(new List<WeatherModel>());

			var logger = new Mock<ILogger<FetchForecast>>();
			var httpClient = MockHttpMessageHandler.CreateClient("{}", HttpStatusCode.OK);

			var service = new FetchForecast(
				geoMock.Object,
				httpClient,
				modelRepo.Object,
				locRepo.Object,
				forecastRepo.Object,
				logger.Object
			);

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.FetchForecastAsync(city));
		}

		[Fact]
		public async Task FetchForecastAsync_throws_when_api_returns_null_hourly()
		{
			var city = "BrokenApiCity";

			var geoMock = new Mock<IGeocodingService>();
			geoMock.Setup(g => g.GetCoordinatesAsync(city)).ReturnsAsync((1.0, 1.0));

			var location = new Location { Id = 99, Name = city };
			var locRepo = new Mock<ILocationRepository>();
			locRepo.Setup(r => r.GetByCityNameAsync(city)).ReturnsAsync(location);

			var forecastRepo = new Mock<IForecastRepository>();
			forecastRepo.Setup(r => r.HasRecentForecastAsync(location.Id, It.IsAny<DateTime>())).ReturnsAsync(false);

			var modelRepo = new Mock<IWeatherModelRepository>();
			modelRepo.Setup(m => m.GetModelsAsync()).ReturnsAsync(new List<WeatherModel> { new WeatherModel { Id = 1, Name = "gfs" } });

			var logger = new Mock<ILogger<FetchForecast>>();

			// API returns malformed object (no "hourly" or hourly null)
			var json = JsonSerializer.Serialize(new { hourly = (object?)null });
			var httpClient = MockHttpMessageHandler.CreateClient(json, HttpStatusCode.OK);

			var service = new FetchForecast(
				geoMock.Object,
				httpClient,
				modelRepo.Object,
				locRepo.Object,
				forecastRepo.Object,
				logger.Object
			);

			await Assert.ThrowsAsync<Exception>(async () => await service.FetchForecastAsync(city));
		}
	}
}
