using Microsoft.EntityFrameworkCore;
using WeatherForecastv2.Data;

namespace WeatherForecastv2.Tests.TestHelpers;

public static class TestDbContextFactory
{
	public static WeatherForecastContext Create()
	{
		var options = new DbContextOptionsBuilder<WeatherForecastContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.EnableSensitiveDataLogging()
			.Options;

		var context = new WeatherForecastContext(options);
		context.Database.EnsureCreated();
		return context;
	}
}
